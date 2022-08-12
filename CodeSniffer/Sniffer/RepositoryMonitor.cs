using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Core.Sniffer;
using CodeSniffer.Core.Source;
using CodeSniffer.Facade;
using CodeSniffer.Plugins;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Reports;
using CodeSniffer.Repository.Source;
using Serilog;
using Serilog.Events;

// TODO keep track of scanning jobs and their logs for frontend monitoring purposes

namespace CodeSniffer.Sniffer
{
    public class RepositoryMonitor : IRepositoryMonitor
    {
        private readonly ILogger logger;
        private readonly IPluginManager pluginManager;
        private readonly ISourceCodeStatusRepository sourceCodeStatusRepository;
        private readonly IReportFacade reportFacade;
        private readonly IJobRunner jobRunner;
        private readonly AppSettings appSettings;
        private static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(1);

        private readonly TimeSpan scanInterval;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private Task? monitorTask;

        private readonly ConcurrentDictionary<string, ICsSourceCodeRepository> cachedSources = new();
        private readonly ConcurrentDictionary<string, IReadOnlyList<string>> cachedSourceGroups = new();
        private readonly ConcurrentDictionary<string, DefinitionInfo> cachedDefinitions = new();

        private IReadOnlyList<GroupedRepository> cachedGroupedRepositories = new List<GroupedRepository>();
        private volatile bool cachedGroupedRepositoriesInvalidated = true;


        public RepositoryMonitor(ILogger logger, IPluginManager pluginManager, ISourceCodeStatusRepository sourceCodeStatusRepository,
            IReportFacade reportFacade, IJobRunner jobRunner, AppSettings appSettings)
        {
            this.logger = logger;
            this.pluginManager = pluginManager;
            this.sourceCodeStatusRepository = sourceCodeStatusRepository;
            this.reportFacade = reportFacade;
            this.jobRunner = jobRunner;
            this.appSettings = appSettings;

            var settingsInterval = TimeSpan.FromMinutes(appSettings.ScanInterval);
            scanInterval = settingsInterval >= MinimumInterval ? settingsInterval : MinimumInterval;
        }


        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            cancellationTokenSource.Cancel();

            if (monitorTask != null)
                await monitorTask;
        }


        public void Initialize(IEnumerable<CsStoredSource> sources, IEnumerable<CsStoredSourceGroup> sourceGroups, IEnumerable<CsStoredDefinition> definitions)
        {
            if (monitorTask != null)
                throw new InvalidOperationException("Initialized must be called exactly once");

            foreach (var source in sources)
            {
                var sourceCodeRepository = GetSourceCodeRepository(source);
                if (sourceCodeRepository != null)
                    cachedSources.TryAdd(source.Id, sourceCodeRepository);
            }

            foreach (var sourceGroup in sourceGroups)
                cachedSourceGroups.TryAdd(sourceGroup.Id, sourceGroup.SourceIds);

            foreach (var definition in definitions)
                cachedDefinitions.TryAdd(definition.Id, new DefinitionInfo(definition.Id, definition.SourceGroupId, definition.Version));

            monitorTask = Task.Factory.StartNew(() => MonitorTask(cancellationTokenSource.Token));
        }


        public void DefinitionChanged(CsStoredDefinition newDefinition)
        {
            var definitionInfo = new DefinitionInfo(newDefinition.Id, newDefinition.SourceGroupId, newDefinition.Version);

            cachedDefinitions.AddOrUpdate(newDefinition.Id, definitionInfo, (_, _) => definitionInfo);
            cachedGroupedRepositoriesInvalidated = true;
        }


        public void DefinitionRemoved(string id)
        {
            if (cachedDefinitions.TryRemove(id, out _))
                cachedGroupedRepositoriesInvalidated = true;
        }


        public void SourceChanged(string id, CsSource newSource)
        {
            var sourceCodeRepository = GetSourceCodeRepository(newSource);
            if (sourceCodeRepository != null)
                cachedSources.AddOrUpdate(id, sourceCodeRepository, (_, _) => sourceCodeRepository);

            cachedGroupedRepositoriesInvalidated = true;
        }


        public void SourceRemoved(string id)
        {
            if (cachedSources.TryRemove(id, out _))
                cachedGroupedRepositoriesInvalidated = true;
        }

        
        public void SourceGroupChanged(string id, CsSourceGroup newSourceGroup)
        {
            cachedSourceGroups.AddOrUpdate(id, newSourceGroup.SourceIds, (_, _) => newSourceGroup.SourceIds);
            cachedGroupedRepositoriesInvalidated = true;
        }


        public void SourceGroupRemoved(string id)
        {
            if (cachedSourceGroups.TryRemove(id, out _))
                cachedGroupedRepositoriesInvalidated = true;
        }


        private async Task MonitorTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                logger.Debug("Starting scan");
                var totalNewRevisions = 0;

                await Parallel.ForEachAsync(
                    GetGroupedRepositories(),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 2,
                        CancellationToken = CancellationToken.None
                    },
                    async (groupedRepository, _) =>
                    {
                        try
                        {
                            var newRevisions = await Scan(groupedRepository.SourceId, groupedRepository.Repository, groupedRepository.Definitions, cancellationToken);
                            Interlocked.Add(ref totalNewRevisions, newRevisions);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Error while scanning source Id {sourceId}: {errorMessage}", groupedRepository.SourceId, e.Message);
                        }
                    });
                

                logger.Write(totalNewRevisions > 0 ? LogEventLevel.Information : LogEventLevel.Debug, "Finished scan");
                try
                {
                    await Task.Delay(scanInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }


        private IEnumerable<GroupedRepository> GetGroupedRepositories()
        {
            if (!cachedGroupedRepositoriesInvalidated)
                return cachedGroupedRepositories;


            IEnumerable<UngroupedRepository> GetSourceGroupRepositories(string definitionId, string sourceGroupId)
            {
                if (!cachedSourceGroups.TryGetValue(sourceGroupId, out var groupSourceIds))
                    yield break;

                foreach (var sourceId in groupSourceIds)
                {
                    if (cachedSources.TryGetValue(sourceId, out var sourceCodeRepository))
                        yield return new UngroupedRepository(definitionId, sourceId, sourceCodeRepository);
                }
            }

            cachedGroupedRepositoriesInvalidated = false;
            cachedGroupedRepositories = cachedDefinitions
                .SelectMany(dsg => GetSourceGroupRepositories(dsg.Key, dsg.Value.SourceGroupId))
                .GroupBy(r => r.SourceId)
                .Select(g =>
                {
                    var first = g.First();
                    return new GroupedRepository(first.SourceId, first.Repository, g.Select(d => cachedDefinitions[d.DefinitionId]).ToList());
                })
                .ToList();

            return cachedGroupedRepositories;
        }


        private async ValueTask<int> Scan(string sourceId, ICsSourceCodeRepository sourceCodeRepository, IReadOnlyList<DefinitionInfo> definitions, CancellationToken cancellationToken)
        {
            logger.Debug("Scanning source Id {sourceId} for new revisions", sourceId);
            var newRevisions = 0;

            await foreach (var revision in sourceCodeRepository.GetRevisions(cancellationToken))
            {
                if (await SkipRevision(sourceId, revision.Name, revision.Id, definitions))
                    continue;

                var workingCopyPath = GetUniquePath(appSettings.CheckoutPath, GetSafeFilename(sourceId + '.' + revision.Id));
                logger.Information("Checking out working copy at {workingCopyPath}", workingCopyPath);
                await sourceCodeRepository.Checkout(revision, workingCopyPath);

                logger.Information("Running scan jobs on {workingCopyPath}", workingCopyPath);
                foreach (var definitionInfo in definitions)
                {
                    var result = await jobRunner.Execute(definitionInfo.DefinitionId, workingCopyPath, cancellationToken);
                    var report = new CsScanReport(definitionInfo.DefinitionId, sourceId, revision.Id, revision.Name, revision.Branch, result.Checks
                        .Select(c => new CsScanReportCheck(c.PluginId, c.Name, c.Report))
                        .ToList());

                    await reportFacade.StoreReport(report);
                }

                await sourceCodeStatusRepository.StoreRevision(sourceId, revision.Id, definitions.Select(d => new RevisionDefinition(d.DefinitionId, d.Version)).ToList());
                newRevisions++;

                try
                {
                    logger.Debug("Removing working copy at {workingCopyPath}", workingCopyPath);
                    Directory.Delete(workingCopyPath, true);
                }
                catch (Exception e)
                {
                    logger.Warning(e, "Failed to remove working copy path {workingCopyPath}", workingCopyPath);
                }
            }

            return newRevisions;
        }


        private async ValueTask<bool> SkipRevision(string sourceId, string revisionName, string revisionId, IEnumerable<DefinitionInfo> definitions)
        {
            var revisionDefinitions = (await sourceCodeStatusRepository.GetRevisionDefinitions(sourceId, revisionId))
                .ToDictionary(p => p.DefinitionId, p => p.Version);

            if (revisionDefinitions.Count == 0)
            {
                logger.Information("Found new revision {revisionName} for source code repository {sourceId}", revisionName, sourceId);
                return false;
            }

            foreach (var definitionInfo in definitions)
            {
                if (!revisionDefinitions.TryGetValue(definitionInfo.DefinitionId, out var scannedVersion))
                {
                    logger.Debug("Previously scanned revision {revisionName} for source code repository {sourceId} not yet scanned by definition {definitionId}, scanning again", 
                        revisionName, sourceId, definitionInfo.DefinitionId);
                    return false;
                }

                if (scannedVersion == definitionInfo.Version) 
                    continue;

                logger.Debug("Previously scanned revision {revisionName} for source code repository {sourceId} was scanned by an older version of definition {definitionId}, scanning again",
                    revisionName, sourceId, definitionInfo.DefinitionId);
                return false;
            }

            // All definitions were scanned using the same version. Definitions may have been removed, but
            // this does not require a rescan.
            return true;
        }


        private ICsSourceCodeRepository? GetSourceCodeRepository(CsSource source)
        {
            return pluginManager.ById(source.PluginId)?.Plugin is ICsSourceCodeRepositoryPlugin plugin 
                ? plugin.Create(logger, source.Configuration)
                : null;
        }


        private static string GetSafeFilename(string value)
        {
            var invalidChars = Path.GetInvalidPathChars();
            return new string(value.Where(c => !invalidChars.Contains(c)).ToArray());
        }


        private static string GetUniquePath(string basePath, string subFolder)
        {
            var uniquePath = Path.Combine(basePath, subFolder);

            if (!Directory.Exists(uniquePath))
            {
                Directory.CreateDirectory(uniquePath);
                return uniquePath;
            }


            var counter = 1;
            do
            {
                uniquePath = Path.Combine(basePath, subFolder + '_' + counter);

                if (!Directory.Exists(uniquePath))
                {
                    Directory.CreateDirectory(uniquePath);
                    return uniquePath;
                }

                counter++;
            } while (true);
        }


        private class GroupedRepository
        {
            public string SourceId { get; }
            public ICsSourceCodeRepository Repository { get; }
            public IReadOnlyList<DefinitionInfo> Definitions { get; }


            public GroupedRepository(string sourceId, ICsSourceCodeRepository repository, IReadOnlyList<DefinitionInfo> definitions)
            {
                SourceId = sourceId;
                Repository = repository;
                Definitions = definitions;
            }
        }


        private class UngroupedRepository
        {
            public string DefinitionId { get; }
            public string SourceId { get; }
            public ICsSourceCodeRepository Repository { get; }


            public UngroupedRepository(string definitionId, string sourceId, ICsSourceCodeRepository repository)
            {
                DefinitionId = definitionId;
                SourceId = sourceId;
                Repository = repository;
            }
        }


        private class CsScanReport : ICsScanReport
        {
            public string DefinitionId { get; }
            public string SourceId { get; }
            public string RevisionId { get; }
            public string RevisionName { get; }
            public string Branch { get; }
            public IReadOnlyList<ICsScanReportCheck> Checks { get; }


            public CsScanReport(string definitionId, string sourceId, string revisionId, string revisionName, string branch, IReadOnlyList<ICsScanReportCheck> checks)
            {
                DefinitionId = definitionId;
                SourceId = sourceId;
                RevisionId = revisionId;
                RevisionName = revisionName;
                Branch = branch;
                Checks = checks;
            }
        }


        private class CsScanReportCheck : ICsScanReportCheck
        {
            public Guid PluginId { get; }
            public string Name { get; }
            public ICsReport Report { get; }


            public CsScanReportCheck(Guid pluginId, string name, ICsReport report)
            {
                PluginId = pluginId;
                Name = name;
                Report = report;
            }
        }


        private class DefinitionInfo
        {
            public string DefinitionId { get; }
            public string SourceGroupId { get; }
            public int Version { get; }


            public DefinitionInfo(string definitionId, string sourceGroupId, int version)
            {
                DefinitionId = definitionId;
                SourceGroupId = sourceGroupId;
                Version = version;
            }
        }
    }
}
