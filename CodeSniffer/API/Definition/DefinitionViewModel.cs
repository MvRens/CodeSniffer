namespace CodeSniffer.API.Definition
{
    public class DefinitionViewModel
    {
        public string? Name { get; set; }
        public DefinitionSourceViewModel[]? Sources { get; set; }
        public DefinitionCheckViewModel[]? Checks { get; set; }
    }


    public class DefinitionSourceViewModel
    {
        public string? Name { get; set; }
        public string? PluginName { get; set; }
        public string? Configuration { get; set; }
    }


    public class DefinitionCheckViewModel
    {
        public string? Name { get; set; }
        public string? PluginName { get; set; }
        public string? Configuration { get; set; }
    }
}
