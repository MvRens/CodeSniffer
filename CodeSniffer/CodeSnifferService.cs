using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CodeSniffer
{
    public class CodeSnifferService : IHostedService
    {
        private readonly ILogger logger;


        public CodeSnifferService(ILogger logger)
        {
            this.logger = logger;
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Information("Service starting...");

            //

            logger.Information("Service started");

            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Information("Service stopping...");

            //

            logger.Information("Service stopped");

            return Task.CompletedTask;
        }
    }
}