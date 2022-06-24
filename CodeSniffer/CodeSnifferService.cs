using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Facade;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Sniffer;
using Microsoft.Extensions.Hosting;
using Serilog;
using SimpleInjector;

namespace CodeSniffer
{
    public class CodeSnifferService : IHostedService
    {
        private readonly ILogger logger;
        private readonly Container container;


        public CodeSnifferService(ILogger logger, Container container)
        {
            this.logger = logger;
            this.container = container;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Information("Service starting...");

            logger.Information("Loading definitions...");
            await container.GetInstance<IDefinitionFacade>().Initialize();

            logger.Information("Service started");
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