using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Core.Sniffer;
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


        private static readonly ICsReport EmptyReport = CsReportBuilder.Create().Build();


        public JobRunner(ILogger logger, IDefinitionRepository definitionRepository, IPluginManager pluginManager)
        {
            this.logger = logger;
            this.definitionRepository = definitionRepository;
            this.pluginManager = pluginManager;
        }


        public async ValueTask<ICsJobResult> Execute(string definitionId, string workingCopyPath, CancellationToken cancellationToken)
        {
            var jobLogger = logger.ForContext("DefinitionId", definitionId);

            jobLogger.Verbose("Starting job on working copy path {workingCopyPath}", workingCopyPath);
            var definition = await definitionRepository.GetDetails(definitionId);
            var checkReports = new List<CsJobCheck>();

            foreach (var check in definition.Checks)
            {
                logger.Debug("Constructing plugin {pluginId} for check {checkName}", check.PluginId, check.Name);

                if (pluginManager.ById(check.PluginId)?.Plugin is ICsSnifferPlugin plugin)
                {
                    var sniffer = plugin.Create(jobLogger, check.Configuration);
                    var report = await sniffer.Execute(workingCopyPath, cancellationToken);

                    checkReports.Add(new CsJobCheck(check.PluginId, check.Name, report ?? EmptyReport));
                }
                else
                {
                    logger.Error("Not a valid sniffer plugin: {pluginId}", check.PluginId);

                    checkReports.Add(new CsJobCheck(
                        check.PluginId,
                        check.Name, 
                        CsReportBuilder.Create()
                            .AddAsset("invalidPlugin." + check.PluginId, check.Name)
                                .SetResult(CsReportResult.Error)
                                .SetSummary("Not a valid sniffer plugin")
                            .Build()));
                }
            }


            return new CsJobResult(checkReports);
        }


        private class CsJobResult : ICsJobResult
        { 
            public IReadOnlyList<CsJobCheck> Checks { get; }


            public CsJobResult(IReadOnlyList<CsJobCheck> checks)
            {
                Checks = checks;
            }
        }
    }
}
