using System.Collections.Generic;
using LiteDB;

namespace LiteDbDemo.Models
{
	public class Album
	{
		public ObjectId Id { get; set; }

		public int? Year { get; set; }

		public string Title { get; set; }

		public string Artist { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
		public ICollection<Song> Songs { get; init; }
#pragma warning restore CA2227 // Collection properties should be read only

		public override string ToString()
		{
			return $"{Year} - {Title}";
		}
	}
}
