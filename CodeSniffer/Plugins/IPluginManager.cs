using System;
using System.Collections.Generic;

namespace CodeSniffer.Plugins
{
    public interface IPluginManager : IEnumerable<ICsPluginInfo>
    {
        ICsPluginInfo? ById(Guid id);
        IEnumerable<T> ByType<T>() where T : ICsPluginInfo;
    }
}
