using System;
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
        /// The ICsPlugin instance.
        /// </summary>
        public ICsPlugin Plugin { get; }
    }
}
