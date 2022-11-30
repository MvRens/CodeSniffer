using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CodeSniffer.Core.Plugin;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using Nito.AsyncEx;
using Serilog;

namespace CodeSniffer.Plugins
{
    internal delegate ValueTask BeforeLoadFunc(string? oldPluginPath, string newPluginPath, Func<ValueTask> waitForUnload);

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
            var executablePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            var builtinPath = Path.Combine(executablePath, @"builtin");

            await LoadPlugins(builtinPath);
            await LoadPlugins(pluginsPath);
        }


        private async ValueTask LoadPlugins(string path)
        {
            logger.Debug("Scanning path {path} for plugins", path);

            if (!Directory.Exists(path))
            {
                logger.Warning("Plugin path {path} does not exist, skipping", path);
                return;
            }

            foreach (var pluginPath in Directory.EnumerateDirectories(path))
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

                await LoadPlugin(pluginPath, manifestFilename, null);
            }

        }


        private async ValueTask<bool> LoadPlugin(string pluginPath, string manifestFilename, BeforeLoadFunc? onBeforeLoad)
        {
            logger.Debug("Loading plugin from manifest {filename}", manifestFilename);

            await using var manifestStream = new FileStream(manifestFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var manifest = JsonSerializer.Deserialize<Manifest>(manifestStream);

            if (manifest == null)
            {
                logger.Error("Failed to parse manifest file {filename}", manifestFilename);
                return false;
            }

            if (manifest.ContainerId == Guid.Empty)
            {
                logger.Error("No ContainerId defined in manifest file {filename}", manifestFilename);
                return false;
            }

            if (string.IsNullOrWhiteSpace(manifest.EntryPoint))
            {
                logger.Error("No EntryPoint defined in manifest file {filename}", manifestFilename);
                return false;
            }

            var basePath = Path.GetDirectoryName(manifestFilename)!;
            var assemblyFilename = Path.GetFullPath(manifest.EntryPoint, basePath);
            if (!assemblyFilename.StartsWith(basePath))
            {
                logger.Error("Entry point must be contained to the manifest's folder in manifest file {filename}", manifestFilename);
                return false;
            }

            if (!File.Exists(assemblyFilename))
            {
                logger.Error("Entry point {assemblyPath} not found in manifest file {filename}", assemblyFilename, manifestFilename);
                return false;
            }

            try
            {
                var loadedAssembly = loadedAssemblies.GetOrAdd(manifest.ContainerId, _ => new LoadedAssembly(manifest.ContainerId));
                var assemblyPlugins = await loadedAssembly.Load(logger, pluginPath, assemblyFilename, onBeforeLoad);

                foreach (var assemblyPlugin in assemblyPlugins)
                    loadedPlugins.AddOrUpdate(assemblyPlugin.Key, assemblyPlugin.Value, (_, _) => assemblyPlugin.Value);

                return true;
            }
            catch (Exception e)
            {
                logger.Error("Error while loading plugin {filename}: {message}", assemblyFilename, e.Message);
                return false;
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


        public async ValueTask Update(Stream pluginZip)
        {
            var zip = new ZipFile(pluginZip);

            if (zip.GetEntry(ManifestFilename) == null)
                throw new Exception($"Missing {ManifestFilename} in plugin ZIP");

            // Extract the new plugin first
            var extractPath = Path.GetFullPath(Path.Combine(pluginsPath, Guid.NewGuid().ToString("N")));
            var checkedPaths = new HashSet<string>();

            try
            {
                foreach (ZipEntry entry in zip)
                {
                    var entryName = Path.DirectorySeparatorChar != '/'
                        ? entry.Name.Replace('/', Path.DirectorySeparatorChar)
                        : entry.Name;

                    var entryFilename = Path.Combine(extractPath, entryName);
                    var entryPath = entry.IsDirectory ? entryFilename : Path.GetDirectoryName(entryFilename);

                    if (entryPath != null && checkedPaths.Add(entryPath))
                        Directory.CreateDirectory(entryPath);

                    if (entry.IsDirectory)
                        continue;

                    await using var reader = zip.GetInputStream(entry);
                    await using var writer = File.Create(entryFilename);
                    await reader.CopyToAsync(writer);
                }


                if (!await LoadPlugin(extractPath, Path.Combine(extractPath, ManifestFilename),
                    (oldPluginPath, pluginPath, waitForUnload) =>
                    {
                        if (oldPluginPath != null && oldPluginPath != pluginPath)
                            TryDeleteOldPlugin(oldPluginPath, waitForUnload);

                        return ValueTask.CompletedTask;
                    }))
                {
                    Directory.Delete(extractPath, true);

                    // Not ideal, but good enough for now so you can at least see something went wrong in the UI
                    throw new Exception("Failed to load plugin, see log for more information");
                }
            }
            catch
            {
                if (Directory.Exists(extractPath))
                    Directory.Delete(extractPath, true);

                throw;
            }
        }


        private void TryDeleteOldPlugin(string path, Func<ValueTask> waitForUnload)
        {
            Task.Run(async () =>
            {
                logger.Debug("Waiting for replaced plugin from {path} to unload", path);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    await waitForUnload();

                    stopwatch.Stop();
                    logger.Debug("Unloaded replaced plugin from {path} in {elapsed} seconds", path, stopwatch.Elapsed.TotalSeconds);
                }
                catch
                {
                    stopwatch.Stop();
                    logger.Error("Failed to unload replaced plugin from {path} after {elapsed} seconds", path, stopwatch.Elapsed.TotalSeconds);
                    return;
                }

                try
                {
                    Directory.Delete(path, true);
                    logger.Information("Deleted replaced plugin from {path}", path);
                }
                catch
                {
                    logger.Error("Failed to delete replaced plugin from {path}", path);
                }
            });
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

            private string? currentPluginPath;
            private readonly AsyncReaderWriterLock pluginAssemblyLock = new();
            private Assembly? pluginAssembly;
            private PluginLoadContext? pluginContext;
            private WeakReference? pluginContextWeakReference;

            private readonly Dictionary<Guid, LoadedPlugin> loadedPlugins = new();


            public LoadedAssembly(Guid containerId)
            {
                this.containerId = containerId;
            }


            public async ValueTask<IReadOnlyList<KeyValuePair<Guid, ICsPluginInfo>>> Load(ILogger logger, string pluginPath, string assemblyFilename, BeforeLoadFunc? onBeforeLoad)
            {
                using (await pluginAssemblyLock.WriterLockAsync())
                {
                    UnloadLocked();
                    
                    if (onBeforeLoad != null)
                        await onBeforeLoad(currentPluginPath, pluginPath, WaitForUnload);

                    currentPluginPath = pluginPath;

                    return LoadLocked(logger, assemblyFilename);
                }
            }


            public async ValueTask Unload()
            {
                using (await pluginAssemblyLock.WriterLockAsync())
                {
                    UnloadLocked();
                }
            }


            private ValueTask WaitForUnload()
            {
                if (pluginContextWeakReference == null)
                    return ValueTask.CompletedTask;

                for (var attempt = 0; attempt < 10 && pluginContextWeakReference.IsAlive; attempt++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                // In testing it shows the context is still alive, but deleting the assembly still works, so
                // for now we'll assume the garbage collection did it's job. Worst case the csplugin.json should've
                // been removed which prevents the old plugin from being loaded anyways after a restart.
                //if (pluginContextWeakReference.IsAlive)
                    //throw new Exception("Failed to unload plugin context");

                return ValueTask.CompletedTask;
            }


            internal async ValueTask<IDisposable> AcquireReadLock()
            {
                return await pluginAssemblyLock.ReaderLockAsync();
            }


            private IReadOnlyList<KeyValuePair<Guid, ICsPluginInfo>> LoadLocked(ILogger logger, string assemblyFilename)
            {
                if (pluginContext != null)
                    return loadedPlugins.Select(p => new KeyValuePair<Guid, ICsPluginInfo>(p.Key, p.Value)).ToArray();

                pluginContext = new PluginLoadContext(assemblyFilename);
                pluginContextWeakReference = new WeakReference(pluginContext);

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
                        if (!pluginType.IsAssignableTo(typeof(ICsPlugin)))
                        {
                            logger.Error("Plugin class {className} in {filename} has CsPlugin attribute, but does not implement the required ICsPlugin interface and was skipped", pluginType.Name, assemblyFilename);
                            continue;
                        }

                        if (!loadedPlugins.TryGetValue(pluginAttribute.Id, out var loadedPlugin))
                        {
                            loadedPlugin = new LoadedPlugin(this, containerId, pluginAttribute.Id, pluginAttribute.Name);
                            loadedPlugins.Add(pluginAttribute.Id, loadedPlugin);
                        }

                        loadedPlugin.Load(pluginType);
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

                pluginAssembly = null;
                pluginContext?.Unload();
                pluginContext = null;
            }
        }


        private class LoadedPlugin : ICsPluginInfo
        {
            public Guid Id { get; }
            public string Name { get; }
            public Guid ContainerId { get; }

            private readonly LoadedAssembly ownerAssembly;
            private ICsPlugin? pluginInstance;


            public LoadedPlugin(LoadedAssembly ownerAssembly, Guid containerId, Guid id, string name)
            {
                this.ownerAssembly = ownerAssembly;

                Id = id;
                Name = name;
                ContainerId = containerId;
            }


            public async ValueTask<ICsPluginLock> Acquire()
            {
                var acquiredLock = await ownerAssembly.AcquireReadLock();
                if (pluginInstance != null) 
                    return new LoadedPluginLock(pluginInstance, acquiredLock);

                acquiredLock.Dispose();
                throw new PluginUnloadedException(Id, $"Plugin {Id} is unloaded and can not be acquired");

            }


            internal void Load(Type pluginType)
            {
                pluginInstance = (ICsPlugin)Activator.CreateInstance(pluginType)!;
            }


            internal void Unload()
            {
                pluginInstance = null;
            }
        }


        private class LoadedPluginLock : ICsPluginLock
        {
            public ICsPlugin Plugin => plugin ?? throw new ObjectDisposedException(nameof(ICsPluginLock));

            private ICsPlugin? plugin;
            private readonly IDisposable acquiredLock;


            public LoadedPluginLock(ICsPlugin plugin, IDisposable acquiredLock)
            {
                this.acquiredLock = acquiredLock;
                this.plugin = plugin;
            }


            public ValueTask DisposeAsync()
            {
                plugin = null;

                GC.SuppressFinalize(this);
                acquiredLock.Dispose();

                return default;
            }
        }
    }
}
