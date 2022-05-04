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
        public string Id { get; }
        public string Name { get; }
        public string? DefaultOptions { get; }


        public PluginViewModel(string id, string name, string? defaultOptions)
        {
            Id = id;
            Name = name;
            DefaultOptions = defaultOptions;
        }
    }
}
