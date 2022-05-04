using System.Text.Json.Nodes;
using CodeSniffer.Core.Plugin;
using Serilog;

namespace CodeSniffer.Core.Source
{
    /// <summary>
    /// Implements a plugin which can create ICsSourceCodeRepository instances.
    /// </summary>
    public interface ICsSourceCodeRepositoryPlugin : ICsPlugin
    {
        /// <summary>
        /// Create an instance of the ICsSourceCodeRepositoryr represented by this plugin.
        /// </summary>
        /// <param name="logger">A Serilog ILogger instance to which the plugin can log it's output.</param>
        /// <param name="options">The options as provided in the CodeSniffer configuration.</param>
        ICsSourceCodeRepository Create(ILogger logger, JsonObject options);
    }
}
