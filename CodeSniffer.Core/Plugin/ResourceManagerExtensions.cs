using System.Globalization;
using System.Resources;

namespace CodeSniffer.Core.Plugin
{
    /// <summary>
    /// Returns the string in the resource with the specified name.
    /// </summary>
    /// <remarks>
    /// Assumes nameof() is used and will therefore throw an exception if no resource with the specified name is found.
    /// </remarks>
    public delegate string GetStringFunc(string name);


    /// <summary>
    /// Provides convenience methods for working with prefered cultures and string resources.
    /// </summary>
    public static class ResourceManagerExtensions
    {
        /// <summary>
        /// Returns a GetString method for the first support culture in the cultures parameter.
        /// </summary>
        /// <param name="resourceManager">The resource manager for the resource file.</param>
        /// <param name="cultures">A list of accepted cultures, in order of preference.</param>
        /// <returns>A method which can be called to retrieve a string resource in the prefered and supported culture.</returns>
        public static GetStringFunc CreateGetString(this ResourceManager resourceManager, IEnumerable<CultureInfo> cultures)
        {
            foreach (var culture in cultures)
            {
                var resourceSet = resourceManager.GetResourceSet(culture, false, false);
                if (resourceSet == null)
                    continue;

                return name =>
                {
                    var value = resourceSet.GetString(name);
                    if (value != null)
                        return value;

                    // The specific resource set might not have the value, fall back to the
                    // resource manager itself to figure out the fallback.
                    value = resourceManager.GetString(name);
                    if (value != null)
                        return value;

                    throw new KeyNotFoundException($"No string resource with name '{name}' was found");
                };
            }

            return name =>
            {
                var value = resourceManager.GetString(name);
                if (value != null)
                    return value;

                throw new KeyNotFoundException($"No string resource with name '{name}' was found");
            };
        }
    }
}
