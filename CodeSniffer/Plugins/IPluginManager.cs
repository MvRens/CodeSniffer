using System;
using System.Collections.Generic;
using CodeSniffer.Core.Plugin;

namespace CodeSniffer.Plugins
{
    public interface IPluginManager : IEnumerable<ICsPluginInfo>
    {
        ICsPluginInfo? ById(Guid id);
        IEnumerable<ICsPluginInfo> ByType<T>() where T : ICsPlugin;
    }
}
