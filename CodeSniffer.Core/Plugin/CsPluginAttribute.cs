using JetBrains.Annotations;

namespace CodeSniffer.Core.Plugin
{
    /// <summary>
    /// Allows an annotated plugin class to be discoverable by the CodeSniffer service.
    /// </summary>
    /// <remarks>
    /// The annotated class must implement at least the ICsPlugin interface.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public class CsPluginAttribute : Attribute
    {
        /// <summary>
        /// Identifies the plugin for use in configuration files.
        /// </summary>
        /// <remarks>
        /// Should be a JSON-friendly id and stable between versions of the same plugin.
        /// Plugin names must be unique per type of plugin.
        /// </remarks>
        public string Id { get; }


        /// <inheritdoc />
        public CsPluginAttribute(string id)
        {
            Id = id;
        }
    }
}
