using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CodeSniffer.Core.Plugin;
using JetBrains.Annotations;
using Nito.AsyncEx;
using Serilog;

namespace CodeSniffer.Plugins
{
    public class PluginManager : IPluginManager, IAsyncDisposable
    {
        private readonly ILogger logger;
        private readonly string pluginsPath;
        private readonly ConcurrentDictionary<Guid, LoadedAssembly> loadedAssemblies = new();
        private readonly ConcurrentDictionary<Guid, ICsPluginInfo> loadedPlugins = new();

        private const string ManifestFilename = "csplugin.json";


        public PluginManager(ILogger logger, string pluginsPath)
        {
            this.logger = logger;
            this.pluginsPath = pluginsPath;
        }


        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

            foreach (var loadedAssembly in loadedAssemblies.Values)
                await loadedAssembly.Unload();

            loadedPlugins.Clear();
            loadedAssemblies.Clear();
        }


        public async ValueTask Initialize()
        {
            logger.Debug("Scanning path {path} for plugins", pluginsPath);

            if (!Directory.Exists(pluginsPath))
            {
                logger.Warning("Plugin path {path} does not exist, skipping", pluginsPath);
                return;
            }

            foreach (var pluginPath in Directory.EnumerateDirectories(pluginsPath))
            {
                var manifestFilename = Path.Combine(pluginPath, ManifestFilename);
                if (!File.Exists(manifestFilename))
                {
                    #if DEBUG
                    // For debug builds, allow one extra subdirectory
                    manifestFilename = Path.Combine(pluginPath, @"Debug", ManifestFilename);
                    if (!File.Exists(manifestFilename))
                        continue;
                    #else
                    continue;
                    #endif
                }
                
                await LoadPlugin(pluginPath, manifestFilename);
            }
        }


        private async ValueTask LoadPlugin(string pluginPath, string manifestFilename)
        {
            logger.Debug("Loading plugin from manifest {filename}", manifestFilename);

            await using var manifestStream = new FileStream(manifestFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var manifest = JsonSerializer.Deserialize<Manifest>(manifestStream);

            if (manifest == null)
            {
                logger.Error("Failed to parse manifest file {filename}", manifestFilename);
                return;
            }

            if (manifest.ContainerId == Guid.Empty)
            {
                logger.Error("No ContainerId defined in manifest file {filename}", manifestFilename);
                return;
            }

            if (string.IsNullOrWhiteSpace(manifest.EntryPoint))
            {
                logger.Error("No EntryPoint defined in manifest file {filename}", manifestFilename);
                return;
            }

            var basePath = Path.GetDirectoryName(manifestFilename)!;
            var assemblyFilename = Path.GetFullPath(manifest.EntryPoint, basePath);
            if (!assemblyFilename.StartsWith(basePath))
            {
                logger.Error("Entry point must be contained to the manifest's folder in manifest file {filename}", manifestFilename);
                return;
            }

            if (!File.Exists(assemblyFilename))
            {
                logger.Error("Entry point {assemblyPath} not found in manifest file {filename}", assemblyFilename, manifestFilename);
                return;
            }

            try
            {
                var loadedAssembly = loadedAssemblies.GetOrAdd(manifest.ContainerId, _ => new LoadedAssembly(manifest.ContainerId));
                var assemblyPlugins = await loadedAssembly.Load(logger, pluginPath, assemblyFilename);

                foreach (var assemblyPlugin in assemblyPlugins)
                    loadedPlugins.AddOrUpdate(assemblyPlugin.Key, assemblyPlugin.Value, (_, _) => assemblyPlugin.Value);
            }
            catch (Exception e)
            {
                logger.Error("Error while loading plugin {filename}: {message}", assemblyFilename, e.Message);
            }
        }


        public IEnumerator<ICsPluginInfo> GetEnumerator()
        {
            return loadedPlugins.Values.GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public ICsPluginInfo? ById(Guid id)
        {
            return loadedPlugins.TryGetValue(id, out var loadedPlugin)
                ? loadedPlugin
                : null;
        }


        public async IAsyncEnumerable<ICsPluginInfo> ByType<T>() where T : ICsPlugin
        {
            foreach (var loadedPlugin in loadedPlugins.Values)
            {
                await using (var pluginLock = await loadedPlugin.Acquire())
                {
                    if (!pluginLock.Plugin.GetType().IsAssignableTo(typeof(T)))
                        continue;
                }

                yield return loadedPlugin;
            }
        }


        public ValueTask Update(Stream pluginZip)
        {
            throw new NotImplementedException();
        }


        [JsonSerializable(typeof(Manifest))]
        private class Manifest
        {
            public Guid ContainerId { get; [UsedImplicitly] set; }
            public string? EntryPoint { get; [UsedImplicitly] set; }
        }


        private class LoadedAssembly
        {
            private readonly Guid containerId;

            private readonly AsyncReaderWriterLock pluginAssemblyLock = new();
            private Assembly? pluginAssembly;
            private PluginLoadContext? pluginContext;
            
            private readonly Dictionary<Guid, LoadedPlugin> loadedPlugins = new();


            public LoadedAssembly(Guid containerId)
            {
                this.containerId = containerId;
            }


            public async ValueTask<IReadOnlyList<KeyValuePair<Guid, ICsPluginInfo>>> Load(ILogger logger, string pluginPath, string assemblyFilename)
            {
                using (await pluginAssemblyLock.WriterLockAsync())
                {
                    return LoadLocked(logger, pluginPath, assemblyFilename);
                }
            }


            public async ValueTask Unload()
            {
                using (await pluginAssemblyLock.WriterLockAsync())
                {
                    UnloadLocked();
                }
            }


            public async ValueTask Reload(ILogger logger, string pluginPath, string assemblyFilename, Func<ValueTask> onBeforeLoad)
            {
                using (await pluginAssemblyLock.WriterLockAsync())
                {
                    UnloadLocked();
                    await onBeforeLoad();
                    LoadLocked(logger, pluginPath, assemblyFilename);
                }
            }


            internal async ValueTask<IDisposable> AcquireReadLock()
            {
                return await pluginAssemblyLock.ReaderLockAsync();
            }


            private IReadOnlyList<KeyValuePair<Guid, ICsPluginInfo>> LoadLocked(ILogger logger, string pluginPath, string assemblyFilename)
            {
                if (pluginContext != null)
                    return loadedPlugins.Select(p => new KeyValuePair<Guid, ICsPluginInfo>(p.Key, p.Value)).ToArray();

                pluginContext = new PluginLoadContext(assemblyFilename);

                // Always map CodeSniffer.Core to our version
                var coreAssembly = typeof(ICsPlugin).Assembly;
                var coreAssemblyName = coreAssembly.GetName().Name;

                pluginContext.Resolving += (_, name) => name.Name == coreAssemblyName ? coreAssembly : null;
                pluginAssembly = pluginContext.LoadFromAssemblyPath(assemblyFilename);

                var pluginClasses = pluginAssembly
                    .GetTypes()
                    .Where(t => t.GetCustomAttribute<CsPluginAttribute>() != null)
                    .ToList();

                if (pluginClasses.Count > 0)
                {
                    logger.Information("{count} plugins found in {filename}", pluginClasses.Count,
                        assemblyFilename);

                    foreach (var pluginType in pluginClasses)
                    {
                        var pluginAttribute = pluginType.GetCustomAttribute<CsPluginAttribute>()!;
                        if (loadedPlugins.ContainsKey(pluginAttribute.Id))
                            continue;

                        if (!pluginType.IsAssignableTo(typeof(ICsPlugin)))
                        {
                            logger.Error(
                                "Plugin class {className} in {filename} has CsPlugin attribute, but does not implement the required ICsPlugin interface and was skipped",
                                pluginType.Name, assemblyFilename);
                            continue;
                        }

                        var loadedPlugin = new LoadedPlugin(this, pluginPath, containerId, pluginAttribute.Id, pluginAttribute.Name, pluginType);
                        loadedPlugin.Load();

                        loadedPlugins.Add(pluginAttribute.Id, loadedPlugin);
                    }
                }
                else
                {
                    logger.Warning("No plugins found in {filename}", assemblyFilename);
                }

                return loadedPlugins.Select(p => new KeyValuePair<Guid, ICsPluginInfo>(p.Key, p.Value)).ToArray();
            }

    
            private void UnloadLocked()
            {
                foreach (var plugin in loadedPlugins.Values)
                    plugin.Unload();

                pluginContext?.Unload();
            }
        }


        private class LoadedPlugin : ICsPluginInfo
        {
            public Guid Id { get; }
            public string Name { get; }
            public Guid ContainerId { get; }

            private readonly LoadedAssembly ownerAssembly;
            private readonly string pluginPath;
            private readonly Type pluginType;
            private ICsPlugin? pluginInstance;


            public LoadedPlugin(LoadedAssembly ownerAssembly, string pluginPath, Guid containerId, Guid id, string name, Type pluginType)
            {
                this.ownerAssembly = ownerAssembly;
                this.pluginPath = pluginPath;

                Id = id;
                Name = name;
                ContainerId = containerId;
                this.pluginType = pluginType;
            }


            public async ValueTask<ICsPluginLock> Acquire()
            {
                var acquiredLock = await ownerAssembly.AcquireReadLock();
                if (pluginInstance != null) 
                    return new LoadedPluginLock(pluginInstance, acquiredLock);

                acquiredLock.Dispose();
                throw new PluginUnloadedException(Id, $"Plugin {Id} is unloaded and can not be acquired");

            }


            internal void Load()
            {
                if (pluginInstance != null)
                    return;

                pluginInstance = (ICsPlugin)Activator.CreateInstance(pluginType)!;
            }


            internal void Unload()
            {
                pluginInstance = null;
            }
        }


        private class LoadedPluginLock : ICsPluginLock
        {
            public ICsPlugin Plugin { get; }

            private readonly IDisposable acquiredLock;


            public LoadedPluginLock(ICsPlugin plugin, IDisposable acquiredLock)
            {
                this.acquiredLock = acquiredLock;
                Plugin = plugin;
            }


            public ValueTask DisposeAsync()
            {
                GC.SuppressFinalize(this);
                acquiredLock.Dispose();

                return default;
            }
        }
    }
}
