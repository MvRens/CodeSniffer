using System;
using System.Collections.Generic;

namespace CodeSniffer.API.Admin
{
    public class PluginContainerViewModel
    {
        public Guid Id { get; }
        public IReadOnlyList<PluginViewModel> Plugins { get; }


        public PluginContainerViewModel(Guid id, IReadOnlyList<PluginViewModel> plugins)
        {
            Id = id;
            Plugins = plugins;
        }
    }

    public class PluginViewModel
    {
        public Guid Id { get; }
        public string Name { get; }


        public PluginViewModel(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
