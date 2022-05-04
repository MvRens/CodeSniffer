using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Sniffer;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace CodeSniffer
{
    public class CodeSnifferService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IDefinitionRepository definitionRepository;
        private readonly IRepositoryMonitor repositoryMonitor;


        public CodeSnifferService(ILogger logger, IDefinitionRepository definitionRepository, IRepositoryMonitor repositoryMonitor)
        {
            this.logger = logger;
            this.definitionRepository = definitionRepository;
            this.repositoryMonitor = repositoryMonitor;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Information("Service starting...");

            logger.Information("Loading definitions...");

            // TODO should this go via the facade instead?
            var definitions = await definitionRepository.GetAllDetails();
            repositoryMonitor.Initialize(definitions);

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