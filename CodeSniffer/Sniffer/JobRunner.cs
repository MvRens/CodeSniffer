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
        private readonly IJobMonitor jobMonitor;


        private static readonly ICsReport EmptyReport = CsReportBuilder.Create().Build();


        public JobRunner(ILogger logger, IDefinitionRepository definitionRepository, IPluginManager pluginManager, IJobMonitor jobMonitor)
        {
            this.logger = logger;
            this.definitionRepository = definitionRepository;
            this.pluginManager = pluginManager;
            this.jobMonitor = jobMonitor;
        }


        public async ValueTask<ICsJobResult> Execute(string definitionId, string workingCopyPath, ICsScanContext context, CancellationToken cancellationToken)
        {
            var runningJob = jobMonitor.Start(logger.ForContext("DefinitionId", definitionId), JobType.Scan, "TODO job name (include definition name?)");

            runningJob.Logger.Verbose("Starting job on working copy path {workingCopyPath}", workingCopyPath);
            var definition = await definitionRepository.GetDetails(definitionId);
            var checkReports = new List<CsJobCheck>();

            for (var checkIndex = 0; checkIndex < definition.Checks.Count; checkIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                runningJob.SetProgress(checkIndex, definition.Checks.Count);

                var check = definition.Checks[checkIndex];
                runningJob.Logger.Debug("Constructing plugin {pluginId} for check {checkName}", check.PluginId, check.Name);

                var pluginInfo = pluginManager.ById(check.PluginId);
                if (pluginInfo == null)
                {
                    checkReports.Add(ReportInvalidPlugin(logger, check));
                    continue;
                }

                await using var pluginLock = await pluginInfo.Acquire();

                if (pluginLock.Plugin is ICsSnifferPlugin plugin)
                {
                    var sniffer = plugin.Create(runningJob.Logger, check.Configuration);
                    var report = await sniffer.Execute(workingCopyPath, context, cancellationToken);

                    checkReports.Add(new CsJobCheck(check.PluginId, check.Name, report ?? EmptyReport));
                }
                else
                    checkReports.Add(ReportInvalidPlugin(logger, check));
            }

            runningJob.SetProgress(definition.Checks.Count, definition.Checks.Count);
            return new CsJobResult(checkReports);
        }


        private static CsJobCheck ReportInvalidPlugin(ILogger logger, CsDefinitionCheck check)
        {
            logger.Error("Not a valid sniffer plugin: {pluginId}", check.PluginId);

            return new CsJobCheck(
                check.PluginId,
                check.Name,
                CsReportBuilder.Create()
                    .AddAsset("invalidPlugin." + check.PluginId, check.Name)
                    .SetResult(CsReportResult.Error)
                    .SetSummary("Not a valid sniffer plugin")
                    .Build());
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
