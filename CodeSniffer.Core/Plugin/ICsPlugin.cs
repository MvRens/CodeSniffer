using System.Globalization;
using System.Text.Json.Nodes;

namespace CodeSniffer.Core.Plugin
{
    /// <summary>
    /// Minimum required interface for CodeSniffer plugins.
    /// </summary>
    /// <remarks>
    /// You should probably implement one of the descendants as well to do anything useful.
    /// </remarks>
    public interface ICsPlugin
    {
        /// <summary>
        /// The name of the plugin for display purposes.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// The default options displayed when adding this plugin in the UI.
        /// </summary>
        JsonObject? DefaultOptions { get; }
    }


    /// <summary>
    /// Implement to provide help text for the user interface.
    /// </summary>
    public interface ICsPluginHelp : ICsPlugin
    {
        /// <summary>
        /// Returns optional help text explaining the supported configuration options in HTML format.
        /// </summary>
        /// <param name="cultures">A list of requested cultures, ordered by preference.</param>
        /// <returns>The help text in HTML format. Should be in the first requested culture supported by the plugin, or use a fallback culture (preferably English).</returns>
        string? GetOptionsHelpHtml(IReadOnlyList<CultureInfo> cultures);
    }
}
