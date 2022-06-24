using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Core.Source;
using CodeSniffer.Plugins;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Source;
using Serilog;

namespace CodeSniffer.Sniffer
{
    public class RepositoryMonitor : IRepositoryMonitor
    {
        private readonly ILogger logger;
        private readonly IPluginManager pluginManager;
        private readonly ISourceCodeStatusRepository sourceCodeStatusRepository;
        private readonly IJobRunner jobRunner;
        private readonly AppSettings appSettings;
        private static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(1);

        private readonly TimeSpan scanInterval;

        private readonly CancellationTokenSource cancellationTokenSource = new();
        private Task? monitorTask;

        private ConcurrentDictionary<string, IReadOnlyList<ICsSourceCodeRepository>>? sources;


        public RepositoryMonitor(ILogger logger, IPluginManager pluginManager, ISourceCodeStatusRepository sourceCodeStatusRepository, IJobRunner jobRunner, AppSettings appSettings)
        {
            this.logger = logger;
            this.pluginManager = pluginManager;
            this.sourceCodeStatusRepository = sourceCodeStatusRepository;
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


        // ReSharper disable once ParameterHidesMember
        public void Initialize(IEnumerable<CsStoredDefinition> definitions)
        {
            if (monitorTask != null)
                throw new InvalidOperationException("Initialized must be called exactly once");

            sources = new ConcurrentDictionary<string, IReadOnlyList<ICsSourceCodeRepository>>(
                definitions.Select(d => new KeyValuePair<string, IReadOnlyList<ICsSourceCodeRepository>>(d.Id, MapDefinition(d)))
            );

            monitorTask = Task.Factory.StartNew(() => MonitorTask(cancellationTokenSource.Token));
        }


        public void DefinitionChanged(string id, CsDefinition newDefinition)
        {
            if (sources == null)
                return;

            var mappedDefinition = MapDefinition(newDefinition);
            sources.AddOrUpdate(id, mappedDefinition, (_, _) => mappedDefinition);
        }


        public void DefinitionRemoved(string id)
        {
            sources?.TryRemove(id, out _);
        }


        private async Task MonitorTask(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                logger.Debug("Starting scan");

                if (sources != null)
                {
                    var groupedSources = sources
                        .SelectMany(s => s.Value.Select(r => new
                        {
                            DefinitionId = s.Key,
                            Repository = r
                        }))
                        .GroupBy(r => r.Repository.Id)
                        .ToDictionary(g => g.Key, g => new GroupedRepository(
                            g.First().Repository,
                            g.Select(x => x.DefinitionId).ToList()
                        ));


                    await Parallel.ForEachAsync(
                        groupedSources,
                        new ParallelOptions
                        {
                            MaxDegreeOfParallelism = 2,
                            CancellationToken = CancellationToken.None
                        },
                        async (group, _) =>
                        {
                            var (_, groupedRepository) = group;

                            try
                            {
                                await Scan(groupedRepository.Repository, groupedRepository.Definitions, cancellationToken);
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, "Error while scanning repository {sourceCodeRepositoryName}: {errorMessage}", groupedRepository.Repository.Name, e.Message);
                            }
                        });
                }

                // TODO log as information if any new revisions were processed
                logger.Debug("Finished scan");

                try
                {
                    await Task.Delay(scanInterval, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            }
        }


        private async ValueTask Scan(ICsSourceCodeRepository sourceCodeRepository, IReadOnlyList<string> definitions, CancellationToken cancellationToken)
        {
            logger.Debug("Scanning source code repository {sourceCodeRepositoryName} for new revisions", sourceCodeRepository.Name);

            await foreach (var revision in sourceCodeRepository.GetRevisions(cancellationToken))
            {
                if (await sourceCodeStatusRepository.HasRevision(sourceCodeRepository.Id, revision.Id))
                    continue;

                var workingCopyPath = GetUniquePath(appSettings.CheckoutPath, GetSafeFilename(sourceCodeRepository.Id + '.' + revision.Id));
                logger.Information("Found new revision {revisionName} for source code repository {sourceCodeRepositoryName}, checking out working copy at {workingCopyPath}", revision.Name, sourceCodeRepository.Name, workingCopyPath);
                await sourceCodeRepository.Checkout(revision, workingCopyPath);


                foreach (var definitionId in definitions)
                    await jobRunner.Execute(definitionId, workingCopyPath);

                await sourceCodeStatusRepository.StoreRevision(sourceCodeRepository.Id, revision.Id);
            }
        }



        private IReadOnlyList<ICsSourceCodeRepository> MapDefinition(CsDefinition definition)
        {
            return definition.Sources.Select(GetSourceCodeRepository)
                .Where(s => s != null)
                .Select(s => s!)
                .ToList();
        }


        private ICsSourceCodeRepository? GetSourceCodeRepository(CsDefinitionSource source)
        {
            return pluginManager.ByName(source.PluginId)?.Plugin is ICsSourceCodeRepositoryPlugin plugin 
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
            public ICsSourceCodeRepository Repository { get; }
            public IReadOnlyList<string> Definitions { get; }


            public GroupedRepository(ICsSourceCodeRepository repository, IReadOnlyList<string> definitions)
            {
                Repository = repository;
                Definitions = definitions;
            }
        }
    }
}
