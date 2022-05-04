using System.Collections.Generic;
using CodeSniffer.Core.Plugin;

namespace CodeSniffer.Plugins
{
    public interface IPluginManager : IEnumerable<ICsPluginInfo>
    {
        ICsPluginInfo? ByName(string name);
        IEnumerable<T> ByType<T>() where T : ICsPluginInfo;
    }
}
