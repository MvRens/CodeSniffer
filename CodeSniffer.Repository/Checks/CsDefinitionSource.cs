using System.Text.Json.Nodes;

namespace CodeSniffer.Repository.Checks
{
    public class CsDefinitionSource
    {
        public string Name { get; }
        public string PluginName { get; }
        public JsonObject Configuration { get; }


        public CsDefinitionSource(string name, string pluginName, JsonObject configuration)
        {
            Name = name;
            PluginName = pluginName;
            Configuration = configuration;
        }
    }
}
