using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CF.Library.Bootstrap;
using LiteDB;
using LiteDbDemo.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LiteDbDemo
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private const string CollectionName = "albums";

		private readonly ILogger<ApplicationLogic> logger;

		private readonly ApplicationSettings settings;

		public ApplicationLogic(ILogger<ApplicationLogic> logger, IOptions<ApplicationSettings> options)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			try
			{
				RunInternal();

				return Task.FromResult(0);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				logger.LogCritical(e, "Application has failed");
				return Task.FromResult(e.HResult);
			}
		}

		private void RunInternal()
		{
			using var database = CreateDatabase(settings.DatabasePath);

			var collection = database.GetCollection<Album>(CollectionName);

			// Creating indices.
			collection.EnsureIndex(x => x.Title);

			CreateData(collection);
			ReadData(collection);
			UpdateData(collection);
			DeleteData(collection);

			logger.LogInformation("Good bye :)");
		}

		private LiteDatabase CreateDatabase(string dbPath)
		{
			var bsonMapper = new BsonMapper
			{
				EnumAsInteger = true,
				SerializeNullValues = false,
				TrimWhitespace = false,
				EmptyStringToNull = false,
			};

			logger.LogInformation("Creating instance of LiteDB on {LiteDbPath} ...", dbPath);

			var database = new LiteDatabase(dbPath, bsonMapper);

			database.DropCollection(CollectionName);

			return database;
		}

		private void CreateData(ILiteCollection<Album> collection)
		{
			logger.LogInformation("Creating the data ...");

			// We do not set Id property, it will be filled by LiteDB.
			var document1 = new Album
			{
				Year = 1999,
				Title = "Issues",

				Songs = new List<Song>
				{
					new()
					{
						TrackNumber = 1,
						Title = "Dead",
						Length = new TimeSpan(0, 1, 12),
					},

					new()
					{
						TrackNumber = 2,
						Title = "Falling Away From Me",
						Length = new TimeSpan(0, 4, 30),
					},
				},
			};

			var document2 = new Album
			{
				Year = 2000,
				Title = "Don't Give Me Names",

				Songs = new List<Song>
				{
					new()
					{
						TrackNumber = 1,
						Title = "Innocent Greed",
						Length = new TimeSpan(0, 3, 51),
					},
				},
			};

			collection.Insert(document1);
			collection.Insert(document2);

			// Bulk insert is also possible:
			// collection.InsertBulk(new[] { document1, document2, });
		}

		private void ReadData(ILiteCollection<Album> collection)
		{
			logger.LogInformation("Reading the data ...");

			// Reading all documents.
			var allAlbums = collection.FindAll().ToList();

			// Reading document by id.
			var albumById = collection.FindById(allAlbums.Last().Id);
			logger.LogInformation("Found album by id: {Album}", albumById);

			// Reading document by any other field.
			var albumByTitle = collection.FindOne(x => x.Title == "Issues");
			logger.LogInformation("Found album by title: {Album}", albumByTitle);

			// Reading document by expression.
			var albums1 = collection.Find(x => x.Year >= 2000);
			logger.LogInformation("Found albums by expression: {Albums}", albums1);

			// Can query nested documents.
			var albums2 = collection.Find(x => x.Songs.Select(s => s.Title).Any(t => t == "Falling Away From Me"));
			logger.LogInformation("Found albums by nested document: {Albums}", albums2);

			// Aggregation
			var latestYear = collection.Max(x => x.Year);
			logger.LogInformation("Latest album year: {Year}", latestYear);
		}

		private void UpdateData(ILiteCollection<Album> collection)
		{
			logger.LogInformation("Updating the data ...");

			var updatedAlbum = collection
				.Include(x => x.Songs)
				.FindOne(x => x.Title == "Issues");

			updatedAlbum.Year = 2021;
			updatedAlbum.Songs.Add(new Song
			{
				TrackNumber = 3,
				Title = "Trash",
				Length = new TimeSpan(0, 3, 27),
			});

			collection.Update(updatedAlbum);

			// Partial update of multiple documents.
			collection.UpdateMany(album => new Album { Title = "Updated: " + album.Title }, album => album.Year >= 2020);
		}

		private void DeleteData(ILiteCollection<Album> collection)
		{
			logger.LogInformation("Deleting the data ...");

			var id = collection.FindAll().First().Id;

			// Deleting document by id.
			collection.Delete(id);

			// Deleting multiple documents by expression;
			collection.DeleteMany(x => x.Year >= 2015);

			// Deleting all documents.
			collection.DeleteAll();
		}
	}
}
