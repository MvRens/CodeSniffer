using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Facade;
using CodeSniffer.Plugins;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Source;
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



            // For mock purposes
            // ------
            var pluginManager = container.GetInstance<IPluginManager>();
            var sourceRepository = container.GetInstance<ISourceRepository>();
            var definitionRepository = container.GetInstance<IDefinitionRepository>();

            if ((await sourceRepository.ListSources()).Count == 0)
            {
                logger.Warning("ADDING MOCK DATA");

                var gitPluginId = new Guid("a7c87e17-10eb-4c69-b515-9eff5710d457");
                var dotnetVersionPluginId = new Guid("fe7d8b97-efd3-44c2-81b6-5f86c0c5f684");


                var dotnetVersionPlugin = pluginManager.ById(dotnetVersionPluginId);


                var sourceId = await sourceRepository.InsertSource(new CsSource("Tapeti", gitPluginId,
                    new JsonObject(new[]
                    {
                        new KeyValuePair<string, JsonNode?>("Url", "https://github.com/MvRens/Tapeti.git")
                    })), "Mock");


                var sourceGroupId =
                    await sourceRepository.InsertSourceGroup(new CsSourceGroup("Github projects", new[] { sourceId }),
                        "Mock");


                await definitionRepository.Insert(new CsDefinition(
                    ".NET version",
                    sourceGroupId,
                    new[]
                    {
                        new CsDefinitionCheck("Obsolete versions", dotnetVersionPluginId,
                            dotnetVersionPlugin!.Plugin.DefaultOptions!)
                    }
                ), "Mock");
            }
            // ------



            logger.Information("Loading configuration...");
            await container.GetInstance<IConfigurationFacade>().Initialize();

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