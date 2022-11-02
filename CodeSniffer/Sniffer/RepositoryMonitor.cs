using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
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
// TODO scan for active branches to clean up every once in a while

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
        private readonly IJobMonitor jobMonitor;
        private static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(1);

        private readonly TimeSpan scanInterval;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private Task? monitorTask;

        private readonly ConcurrentDictionary<string, CachedSource> cachedSources = new();
        private readonly ConcurrentDictionary<string, IReadOnlyList<string>> cachedSourceGroups = new();
        private readonly ConcurrentDictionary<string, DefinitionInfo> cachedDefinitions = new();

        private IReadOnlyList<GroupedRepository> cachedGroupedRepositories = new List<GroupedRepository>();
        private volatile bool cachedGroupedRepositoriesInvalidated = true;


        public RepositoryMonitor(ILogger logger, IPluginManager pluginManager, ISourceCodeStatusRepository sourceCodeStatusRepository,
            IReportFacade reportFacade, IJobRunner jobRunner, AppSettings appSettings, IJobMonitor jobMonitor)
        {
            this.logger = logger;
            this.pluginManager = pluginManager;
            this.sourceCodeStatusRepository = sourceCodeStatusRepository;
            this.reportFacade = reportFacade;
            this.jobRunner = jobRunner;
            this.appSettings = appSettings;
            this.jobMonitor = jobMonitor;

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


        public async ValueTask Initialize(IEnumerable<CsStoredSource> sources, IEnumerable<CsStoredSourceGroup> sourceGroups, IEnumerable<CsStoredDefinition> definitions)
        {
            if (monitorTask != null)
                throw new InvalidOperationException("Initialized must be called exactly once");

            foreach (var source in sources)
            {
                var sourceCodeRepositoryPluginInfo = await GetSourceCodeRepositoryPluginInfo(source);
                if (sourceCodeRepositoryPluginInfo != null)
                    cachedSources.TryAdd(source.Id, new CachedSource(source.Name, source.Configuration, sourceCodeRepositoryPluginInfo));
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


        public async ValueTask SourceChanged(string id, CsSource newSource)
        {
            var sourceCodeRepositoryPluginInfo = await GetSourceCodeRepositoryPluginInfo(newSource);
            if (sourceCodeRepositoryPluginInfo != null)
            {
                var source = new CachedSource(newSource.Name, newSource.Configuration, sourceCodeRepositoryPluginInfo);
                cachedSources.AddOrUpdate(id, source, (_, _) => source);
            }

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
                        using var checkRevisionsJob = jobMonitor.Start(logger, JobType.CheckRevisions, string.Format(Strings.JobNameScan, groupedRepository.Source.Name));
                        try
                        {
                            await using var pluginLock = await groupedRepository.Source.RepositoryPluginInfo.Acquire();
                            var sourceCodeRepositoryPlugin = ((ICsSourceCodeRepositoryPlugin)pluginLock.Plugin).Create(logger, groupedRepository.Source.Configuration);

                            var newRevisions = await Scan(checkRevisionsJob, groupedRepository.SourceId, sourceCodeRepositoryPlugin, groupedRepository.Definitions, cancellationToken);
                            Interlocked.Add(ref totalNewRevisions, newRevisions);
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception e)
                        {
                            checkRevisionsJob.Logger.Error(e, "Error while scanning source Id {sourceId}: {errorMessage}", groupedRepository.SourceId, e.Message);
                            checkRevisionsJob.SetStatus(JobStatus.Error);
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
                    if (cachedSources.TryGetValue(sourceId, out var cachedSource))
                        yield return new UngroupedRepository(definitionId, sourceId, cachedSource);
                }
            }

            cachedGroupedRepositoriesInvalidated = false;
            cachedGroupedRepositories = cachedDefinitions
                .SelectMany(dsg => GetSourceGroupRepositories(dsg.Key, dsg.Value.SourceGroupId))
                .GroupBy(r => r.SourceId)
                .Select(g =>
                {
                    var first = g.First();
                    return new GroupedRepository(first.SourceId, first.Source, g.Select(d => cachedDefinitions[d.DefinitionId]).ToList());
                })
                .ToList();

            return cachedGroupedRepositories;
        }


        private async ValueTask<int> Scan(IRunningJob checkRevisionsJob, string sourceId, ICsSourceCodeRepository sourceCodeRepository, IReadOnlyList<DefinitionInfo> definitions, CancellationToken cancellationToken)
        {
            var newRevisions = 0;
            checkRevisionsJob.Logger.Debug("Scanning source Id {sourceId} for new revisions", sourceId);

            var revisions = await sourceCodeRepository.GetRevisions(cancellationToken).ToListAsync(cancellationToken);

            for (var revisionIndex = 0; revisionIndex < revisions.Count; revisionIndex++)
            {
                checkRevisionsJob.SetProgress(revisionIndex, revisions.Count);
                var revision = revisions[revisionIndex];

                if (await SkipRevision(checkRevisionsJob.Logger, sourceId, revision.Name, revision.Id, definitions))
                    continue;

                var workingCopyPath = GetUniquePath(appSettings.CheckoutPath, GetSafeFilename(sourceId + '.' + revision.Id));
                checkRevisionsJob.Logger.Information("Checking out working copy at {workingCopyPath}", workingCopyPath);
                await sourceCodeRepository.Checkout(revision, workingCopyPath);

                checkRevisionsJob.Logger.Information("Running scan jobs on {workingCopyPath}", workingCopyPath);
                foreach (var definitionInfo in definitions)
                {
                    var result = await jobRunner.Execute(definitionInfo.DefinitionId, workingCopyPath, cancellationToken);
                    var report = new CsScanReport(definitionInfo.DefinitionId, sourceId, revision.Id, revision.Name, revision.Branch, result.Checks
                        .Select(c => new CsScanReportCheck(c.PluginId, c.Name, c.Report))
                        .ToList());

                    await reportFacade.StoreReport(report);
                    cancellationToken.ThrowIfCancellationRequested();
                }

                await sourceCodeStatusRepository.StoreRevision(sourceId, revision.Id, definitions.Select(d => new RevisionDefinition(d.DefinitionId, d.Version)).ToList());
                newRevisions++;

                DeleteWorkingCopy(checkRevisionsJob.Logger, workingCopyPath);
            }

            checkRevisionsJob.SetProgress(revisions.Count, revisions.Count);
            return newRevisions;
        }


        private static void DeleteWorkingCopy(ILogger logger, string workingCopyPath)
        {
            try
            {
                logger.Debug("Removing working copy at {workingCopyPath}", workingCopyPath);
                DeleteDirectory(workingCopyPath);
            }
            catch (Exception e)
            {
                logger.Warning(e, "Failed to remove working copy path {workingCopyPath}", workingCopyPath);
            }
        }


        // https://github.com/libgit2/libgit2sharp/issues/1354#issuecomment-277936895
        private static void DeleteDirectory(string directory)
        {
            foreach (var subdirectory in Directory.EnumerateDirectories(directory))
                DeleteDirectory(subdirectory);

            foreach (var fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName)
                {
                    Attributes = FileAttributes.Normal
                };
                fileInfo.Delete();
            }

            Directory.Delete(directory);
        }


        private async ValueTask<bool> SkipRevision(ILogger jobLog, string sourceId, string revisionName, string revisionId, IEnumerable<DefinitionInfo> definitions)
        {
            var revisionDefinitions = (await sourceCodeStatusRepository.GetRevisionDefinitions(sourceId, revisionId))
                .ToDictionary(p => p.DefinitionId, p => p.Version);

            if (revisionDefinitions.Count == 0)
            {
                jobLog.Information("Found new revision {revisionName} for source code source {sourceId}", revisionName, sourceId);
                return false;
            }

            foreach (var definitionInfo in definitions)
            {
                if (!revisionDefinitions.TryGetValue(definitionInfo.DefinitionId, out var scannedVersion))
                {
                    jobLog.Debug("Previously scanned revision {revisionName} for source code source {sourceId} not yet scanned by definition {definitionId}, scanning again", 
                        revisionName, sourceId, definitionInfo.DefinitionId);
                    return false;
                }

                if (scannedVersion != definitionInfo.Version)
                {
                    jobLog.Debug(
                        "Previously scanned revision {revisionName} for source code source {sourceId} was scanned by an older version of definition {definitionId}, scanning again",
                        revisionName, sourceId, definitionInfo.DefinitionId);
                    return false;
                }

                jobLog.Debug("Previously scanned revision {revisionName} for source code source {sourceId} already scanned by the same version of {definitionId}",
                        revisionName, sourceId, definitionInfo.DefinitionId);
            }

            // All definitions were scanned using the same version. Definitions may have been removed, but
            // this does not require a rescan.
            return true;
        }


        private async ValueTask<ICsPluginInfo?> GetSourceCodeRepositoryPluginInfo(CsSource source)
        {
            var pluginInfo = pluginManager.ById(source.PluginId);
            if (pluginInfo == null)
                return null;

            await using var pluginLock = await pluginInfo.Acquire();
            return pluginLock.Plugin is ICsSourceCodeRepositoryPlugin
                ? pluginInfo
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


        private class CachedSource
        {
            public string Name { get; }
            public JsonObject Configuration { get; }
            public ICsPluginInfo RepositoryPluginInfo { get; }


            public CachedSource(string name, JsonObject configuration, ICsPluginInfo repositoryPluginInfo)
            {
                Name = name;
                Configuration = configuration;
                RepositoryPluginInfo = repositoryPluginInfo;
            }
        }

        private class GroupedRepository
        {
            public string SourceId { get; }
            public CachedSource Source { get; }
            public IReadOnlyList<DefinitionInfo> Definitions { get; }


            public GroupedRepository(string sourceId, CachedSource source, IReadOnlyList<DefinitionInfo> definitions)
            {
                SourceId = sourceId;
                Source = source;
                Definitions = definitions;
            }
        }


        private class UngroupedRepository
        {
            public string DefinitionId { get; }
            public string SourceId { get; }
            public CachedSource Source { get; }


            public UngroupedRepository(string definitionId, string sourceId, CachedSource source)
            {
                DefinitionId = definitionId;
                SourceId = sourceId;
                Source = source;
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
