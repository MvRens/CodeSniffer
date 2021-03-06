using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeSniffer.Core.Plugin;
using JetBrains.Annotations;
using Serilog;

namespace CodeSniffer.Plugins
{
    public class PluginManager : IPluginManager, IDisposable
    {
        private readonly ILogger logger;
        private readonly Dictionary<Guid, LoadedPlugin> loadedPlugins = new();

        //private string[]? pluginPaths;


        public PluginManager(ILogger logger)
        {
            this.logger = logger;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
            UnloadPlugins();
        }

        private void UnloadPlugins()
        {
            foreach (var loadedPlugin in loadedPlugins.Values)
                loadedPlugin.Unload();

            loadedPlugins.Clear();
        }


        // ReSharper disable once ParameterHidesMember
        public void Initialize(string[] pluginPaths)
        {
            // TODO monitor paths for changes and reload automatically - be careful with plugin references though, only reload when the service is not using any plugin
            //this.pluginPaths = pluginPaths;

            UnloadPlugins();
            
            foreach (var path in pluginPaths)
                LoadPlugins(path);
        }


        private void LoadPlugins(string path)
        {
            logger.Debug("Scanning path {path} for plugins", path);

            foreach (var manifestFilename in Directory.GetFiles(path, "csplugin.json", SearchOption.AllDirectories))
                LoadPlugin(manifestFilename);
        }


        private void LoadPlugin(string manifestFilename)
        {
            logger.Debug("Loading plugin from manifest {filename}", manifestFilename);

            using var manifestStream = new FileStream(manifestFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var manifest = JsonSerializer.Deserialize<Manifest>(manifestStream);

            if (manifest == null)
            {
                logger.Error("Failed to parse manifest file {filename}", manifestFilename);
                return;
            }

            if (string.IsNullOrWhiteSpace(manifest.EntryPoint))
            {
                logger.Error("No entry point defined in manifest file {filename}", manifestFilename);
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
                var pluginContext = new PluginLoadContext(assemblyFilename);

                // Always map CodeSniffer.Core to our version
                var coreAssembly = typeof(ICsPlugin).Assembly;
                var coreAssemblyName = coreAssembly.GetName().Name;

                pluginContext.Resolving += (_, name) => name.Name == coreAssemblyName ? coreAssembly : null;

                var keepContext = false;
                try
                {
                    var pluginAssembly = pluginContext.LoadFromAssemblyPath(assemblyFilename);

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
                            {
                                logger.Warning("Duplicate plugin name {name}, version from {filename} skipped",
                                    pluginAttribute.Id, assemblyFilename);
                                continue;
                            }

                            if (!pluginType.IsAssignableTo(typeof(ICsPlugin)))
                            {
                                logger.Error(
                                    "Plugin class {className} in {filename} has CsPlugin attribute, but does not implement the required ICsPlugin interface and was skipped",
                                    pluginType.Name, assemblyFilename);
                                continue;
                            }

                            var pluginInstance = (ICsPlugin)Activator.CreateInstance(pluginType)!;
                            loadedPlugins.Add(pluginAttribute.Id, new LoadedPlugin(pluginContext, new PluginInfo(pluginAttribute.Id, pluginInstance)));
                            keepContext = true;
                        }
                    }
                    else
                    {
                        logger.Warning("No plugins found in {filename}", assemblyFilename);
                    }
                }
                finally
                {
                    if (!keepContext)
                        pluginContext.Unload();
                }
            }
            catch (Exception e)
            {
                logger.Error("Error while loading plugin {filename}: {message}", assemblyFilename, e.Message);
            }
        }


        public IEnumerator<ICsPluginInfo> GetEnumerator()
        {
            return loadedPlugins.Values.Select(p => p.Info).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public ICsPluginInfo? ById(Guid id)
        {
            return loadedPlugins.TryGetValue(id, out var loadedPlugin)
                ? loadedPlugin.Info
                : null;
        }


        public IEnumerable<ICsPluginInfo> ByType<T>() where T : ICsPlugin
        {
            return loadedPlugins.Values
                .Where(p => p.Info.Plugin.GetType().IsAssignableTo(typeof(T)))
                .Select(p => p.Info)
                .ToList();
        }


        [JsonSerializable(typeof(Manifest))]
        private class Manifest
        {
            public string? EntryPoint { get; [UsedImplicitly] set; }
        }


        private readonly struct LoadedPlugin
        {
            private readonly PluginLoadContext loadContext;
            public ICsPluginInfo Info { get; }


            public LoadedPlugin(PluginLoadContext loadContext, ICsPluginInfo info)
            {
                this.loadContext = loadContext;
                Info = info;
            }

            public void Unload()
            {
                loadContext.Unload();
            }
        }


        private class PluginInfo : ICsPluginInfo
        {
            public Guid Id { get; }
            public ICsPlugin Plugin { get; }


            public PluginInfo(Guid id, ICsPlugin plugin)
            {
                Plugin = plugin;
                Id = id;
            }
        }
    }
}
