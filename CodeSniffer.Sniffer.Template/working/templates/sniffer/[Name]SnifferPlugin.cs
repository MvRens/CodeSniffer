using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CodeSniffer.Core.Plugin;
using CodeSniffer.Core.Sniffer;
using JetBrains.Annotations;
using Serilog;

namespace Sniffer.[Name]
{
    [CsPlugin("[PluginId]", "[Name]")]
    [UsedImplicitly]
    public class [Name]SnifferPlugin : ICsSnifferPlugin, ICsPluginHelp
    {
        public JsonObject? DefaultOptions => JsonSerializer.SerializeToNode([Name]Options.Default()) as JsonObject;


        public ICsSniffer Create(ILogger logger, JsonObject options)
        {
            var [Name]Options = options.Deserialize<[Name]Options>();
            CsOptionMissingException.ThrowIfNull([Name]Options);

            return new [Name]Sniffer(logger, [Name]Options);
        }


        public string GetOptionsHelpHtml(IReadOnlyList<CultureInfo> cultures)
        {
            var getString = Strings.ResourceManager.CreateGetString(cultures);

            return CsPluginHelpBuilder.Create()
                .SetSummary(getString(nameof(Strings.HelpSummary)))
                //.AddConfiguration(nameof([Name]Options.OptionName), getString(nameof(Strings.HelpOptionNameSummary)))
                .BuildHtml();
        }
    }
}