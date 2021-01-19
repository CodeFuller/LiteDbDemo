using System;
using System.Threading;
using System.Threading.Tasks;
using CF.Library.Bootstrap;
using Microsoft.Extensions.Logging;

namespace LiteDbDemo
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private readonly ILogger<ApplicationLogic> logger;

		public ApplicationLogic(ILogger<ApplicationLogic> logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			try
			{
				await RunInternal(cancellationToken);

				return 0;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				logger.LogCritical(e, "Application has failed");
				return e.HResult;
			}
		}

		private async Task RunInternal(CancellationToken cancellationToken)
		{
			logger.LogInformation("Hello :)");

			await Task.Delay(TimeSpan.Zero, cancellationToken);
		}
	}
}
