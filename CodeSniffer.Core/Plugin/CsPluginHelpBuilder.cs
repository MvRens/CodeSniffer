using System.Text;
using System.Web;

namespace CodeSniffer.Core.Plugin
{
    /// <summary>
    /// Provides a standardized way to build help text for the user interface.
    /// </summary>
    public class CsPluginHelpBuilder
    {
        /// <summary>
        /// The summary which is displayed before describing the supported options.
        /// </summary>
        public string? Summary { get; set; }

        private readonly List<ConfigurationHelp> configuration = new();


        private CsPluginHelpBuilder()
        {
        }


        /// <summary>
        /// Create a new CsPluginHelpBuilder instance.
        /// </summary>
        /// <remarks>
        /// Call <see cref="BuildHtml"/> to build the help text.
        /// </remarks>
        public static CsPluginHelpBuilder Create()
        {
            return new CsPluginHelpBuilder();
        }


        /// <inheritdoc cref="Summary" />
        public CsPluginHelpBuilder SetSummary(string summary)
        {
            Summary = summary;
            return this;
        }


        /// <summary>
        /// Adds help text for a configuration option.
        /// </summary>
        /// <param name="key">The key in the JSON configuration. Separate with dots (.) to indicate child options.</param>
        /// <param name="required">Indicates if the configuration option is required.</param>
        /// <param name="summary">A short text describing the option.</param>
        /// <param name="description">An optional full description of the option.</param>
        public CsPluginHelpBuilder AddConfiguration(string key, string summary, string? description = null, bool required = false)
        {
            configuration.Add(new ConfigurationHelp(key, required, summary, description));
            return this;
        }


        /// <summary>
        /// Builds the HTML help text.
        /// </summary>
        public string BuildHtml()
        {
            var output = new StringBuilder();

            if (!string.IsNullOrEmpty(Summary))
                output.AppendLine($"<span class=\"help-summary\">{HttpUtility.HtmlEncode(Summary)}</span>");

            // ReSharper disable once InvertIf
            if (configuration.Count > 0)
            {
                output.AppendLine("<div class=\"help-configuration\">");

                foreach (var option in configuration)
                {
                    output
                        .AppendLine($"<div class=\"help-option{(option.Required ? " required" : "")}\">")
                        .AppendLine($"  <div class=\"help-option-key\">{HttpUtility.HtmlEncode(option.Key)}</div>");

                    output.AppendLine($"  <div class=\"help-option-summary\">{HttpUtility.HtmlEncode(option.Summary)}</div>");

                    if (!string.IsNullOrEmpty(option.Description))
                        output.AppendLine($"  <div class=\"help-option-description\">{HttpUtility.HtmlEncode(option.Description)}</div>");

                    output.AppendLine("</div>");
                }

                output.AppendLine("</div>");
            }

            return output.ToString();
        }


        private class ConfigurationHelp
        {
            public string Key { get; }
            public bool Required { get; }
            public string Summary { get; }
            public string? Description { get; }


            public ConfigurationHelp(string key, bool required, string summary, string? description)
            {
                Key = key;
                Required = required;
                Summary = summary;
                Description = description;
            }
        }
    }
}
