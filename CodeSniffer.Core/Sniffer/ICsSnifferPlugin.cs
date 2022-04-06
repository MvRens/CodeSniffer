using System.Text.Json.Nodes;
using CodeSniffer.Core.Plugin;
using Serilog;

namespace CodeSniffer.Core.Sniffer
{
    /// <summary>
    /// Implements a plugin which can create ICsSniffer instances.
    /// </summary>
    public interface ICsSnifferPlugin : ICsPlugin
    {
        /// <summary>
        /// Create an instance of the ICsSniffer represented by this plugin.
        /// </summary>
        /// <param name="logger">A Serilog ILogger instance to which the plugin can log it's output.</param>
        /// <param name="options">The options as provided in the CodeSniffer configuration.</param>
        ICsSniffer Create(ILogger logger, JsonObject options);
    }
}
