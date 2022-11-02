using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CodeSniffer.Core.Plugin;

namespace CodeSniffer.Plugins
{
    public interface IPluginManager : IEnumerable<ICsPluginInfo>
    {
        ICsPluginInfo? ById(Guid id);
        IAsyncEnumerable<ICsPluginInfo> ByType<T>() where T : ICsPlugin;

        ValueTask Update(Stream pluginZip);
        //ValueTask Remove(Guid id);
    }


    public class PluginUnloadedException : Exception
    {
        public Guid PluginId { get; }


        public PluginUnloadedException(Guid pluginId, string message) : base(message)
        {
            PluginId = pluginId;
        }
    }
}
