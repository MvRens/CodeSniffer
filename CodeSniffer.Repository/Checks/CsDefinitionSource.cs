using System.Text.Json.Nodes;

namespace CodeSniffer.Repository.Checks
{
    public class CsDefinitionSource
    {
        public string Name { get; }
        public string SourcePluginId { get; }
        public JsonObject Configuration { get; }


        public CsDefinitionSource(string name, string sourcePluginId, JsonObject configuration)
        {
            Name = name;
            SourcePluginId = sourcePluginId;
            Configuration = configuration;
        }
    }
}
