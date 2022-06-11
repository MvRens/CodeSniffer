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
        /// Help text explaining the supported options in HTML format.
        /// </summary>
        string? OptionsHelpHtml { get; }
    }
}
