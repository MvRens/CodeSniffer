using System.Collections.Generic;
using System.Threading.Tasks;
using CodeSniffer.Core.Sniffer;
using CodeSniffer.Facade;
using CodeSniffer.Plugins;
using CodeSniffer.Repository.Checks;
using Serilog;

namespace CodeSniffer.Sniffer
{
    public class JobRunner : IJobRunner
    {
        private readonly ILogger logger;
        private readonly IDefinitionRepository definitionRepository;
        private readonly IPluginManager pluginManager;
        private readonly IJobResultHandler jobResultHandler;


        private static readonly ICsReport EmptyReport = CsReportBuilder.Create().Build();


        public JobRunner(ILogger logger, IDefinitionRepository definitionRepository, IPluginManager pluginManager, IJobResultHandler jobResultHandler)
        {
            this.logger = logger;
            this.definitionRepository = definitionRepository;
            this.pluginManager = pluginManager;
            this.jobResultHandler = jobResultHandler;
        }


        public async ValueTask Execute(string definitionId, string workingCopyPath)
        {
            var jobLogger = logger.ForContext("DefinitionId", definitionId);

            jobLogger.Verbose("Starting job on working copy path {workingCopyPath}", workingCopyPath);
            var definition = await definitionRepository.GetDetails(definitionId);
            var checkReports = new List<CsCheckReport>();

            foreach (var check in definition.Checks)
            {
                logger.Debug("Constructing plugin {pluginName} for check {checkName}", check.PluginName, check.Name);

                if (pluginManager.ByName(check.PluginName) is ICsSnifferPlugin plugin)
                {
                    var sniffer = plugin.Create(jobLogger, check.Configuration);
                    var report = sniffer.Execute(workingCopyPath);

                    checkReports.Add(new CsCheckReport(check.Name, report ?? EmptyReport));
                }
                else
                {
                    logger.Error("Not a valid sniffer plugin: {pluginName}", check.PluginName);

                    checkReports.Add(new CsCheckReport(check.Name, 
                        CsReportBuilder.Create()
                            .AddAsset(check.PluginName)
                                .SetResult(CsReportResult.Error)
                                .SetSummary("Not a valid sniffer plugin")
                            .Build()));
                }
            }


            await jobResultHandler.StoreJobResult(new CsJobResult(checkReports));
        }


        private class CsJobResult : ICsJobResult
        { 
            public IReadOnlyList<CsCheckReport> Checks { get; }


            public CsJobResult(IReadOnlyList<CsCheckReport> checks)
            {
                Checks = checks;
            }
        }
    }
}
