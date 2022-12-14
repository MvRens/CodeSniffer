using System.Text.Json.Serialization;

namespace Sniffer.[Name]
{
    [JsonSerializable(typeof([Name]Options))]
    public class [Name]Options
    {
        // TODO add options


        public static [Name]Options Default()
        {
            return new [Name]Options
            {
                // TODO set defaults
            };
        }
    }
}
