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
        /// Should be stable between versions of the same plugin.
        /// </remarks>
        public Guid Id { get; }


        /// <summary>
        /// The display name of the plugin.
        /// </summary>
        public string Name { get; }


        /// <inheritdoc />
        /// <param name="guid">Must be a valid GUID string. Presented as string to allow for a compile-time constant.</param>
        /// <param name="name"></param>
        public CsPluginAttribute(string guid, string name)
        {
            Id = Guid.Parse(guid);
            Name = name;
        }
    }
}
