using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace CodeSniffer.API
{
    public static class JsonSerializerExtensions
    {
        public static readonly JsonSerializerOptions DisplayFormatSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            WriteIndented = true
        };


        public static string ToDisplayJsonString(this JsonNode node)
        {
            return node.ToJsonString(DisplayFormatSerializerOptions);
        }
    }
}
