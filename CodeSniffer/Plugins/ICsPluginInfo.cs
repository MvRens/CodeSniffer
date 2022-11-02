using System;
using System.Threading.Tasks;
using CodeSniffer.Core.Plugin;

namespace CodeSniffer.Plugins
{
    public interface ICsPluginInfo
    {
        /// <summary>
        /// The unique ID of the plugin.
        /// </summary>
        public Guid Id { get; }


        /// <summary>
        /// The name of the plugin for display purposes.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Returns the unique ID of the container hosting the plugin, as specified in csplugin.json.
        /// </summary>
        Guid ContainerId { get; }


        /// <summary>
        /// Provides access to the ICsPlugin instance.
        /// </summary>
        /// <remarks>
        /// Dispose when the plugin is no longer required. This allows for updating or reloading the plugin.
        /// </remarks>
        public ValueTask<ICsPluginLock> Acquire();
    }


    public interface ICsPluginLock : IAsyncDisposable
    {
        /// <summary>
        /// The ICsPlugin instance.
        /// </summary>
        public ICsPlugin Plugin { get; }
    }
}
