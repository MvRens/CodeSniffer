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
        /// Should be a JSON-friendly name and stable between versions of the same plugin.
        /// Plugin names must be unique per type of plugin.
        /// </remarks>
        public string Name { get; }


        /// <inheritdoc />
        public CsPluginAttribute(string name)
        {
            Name = name;
        }
    }
}
