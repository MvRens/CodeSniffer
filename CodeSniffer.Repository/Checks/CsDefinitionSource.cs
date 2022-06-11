using System.Text.Json.Nodes;

namespace CodeSniffer.Repository.Checks
{
    public class CsDefinitionSource
    {
        public string Name { get; }
        public string PluginId { get; }
        public JsonObject Configuration { get; }


        public CsDefinitionSource(string name, string pluginId, JsonObject configuration)
        {
            Name = name;
            PluginId = pluginId;
            Configuration = configuration;
        }
    }
}
