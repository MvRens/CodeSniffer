using System;

namespace CodeSniffer.API.Definition
{
    public class PluginsViewModel
    {
        public PluginViewModel[] SourcePlugins { get; }
        public PluginViewModel[] CheckPlugins { get; }


        public PluginsViewModel(PluginViewModel[] sourcePlugins, PluginViewModel[] checkPlugins)
        {
            SourcePlugins = sourcePlugins;
            CheckPlugins = checkPlugins;
        }
    }


    public class PluginViewModel
    {
        public Guid Id { get; }
        public string Name { get; }
        public string? DefaultOptions { get; }
        public string? OptionsHelp { get; }


        public PluginViewModel(Guid id, string name, string? defaultOptions, string? optionsHelp)
        {
            Id = id;
            Name = name;
            DefaultOptions = defaultOptions;
            OptionsHelp = optionsHelp;
        }
    }
}
