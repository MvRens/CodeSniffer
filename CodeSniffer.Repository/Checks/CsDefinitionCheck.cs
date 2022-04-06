using System.Text.Json.Nodes;

namespace CodeSniffer.Repository.Checks
{
    public class CsDefinitionCheck
    {
        public string Name { get; }
        public string SourcePluginId { get; }
        public JsonObject Configuration { get; }


        public CsDefinitionCheck(string name, string sourcePluginId, JsonObject configuration)
        {
            Name = name;
            SourcePluginId = sourcePluginId;
            Configuration = configuration;
        }
    }
}
