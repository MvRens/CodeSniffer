using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CodeSniffer.Core.Plugin
{
    /// <summary>
    /// Represents an error in the CodeSniffer configuration.
    /// </summary>
    public class CsOptionValidationException : Exception
    {
        /// <inheritdoc />
        public CsOptionValidationException(string message, string? optionName = null)
            : base(!string.IsNullOrEmpty(optionName) 
                ? $"Failed to validate configuration option {optionName}: {message}" 
                : $"Failed to validate configuration: {message}")
        {

        }
    }


    /// <summary>
    /// Represents a missing option in the CodeSniffer configuration.
    /// </summary>
    public class CsOptionMissingException : CsOptionValidationException
    {
        /// <inheritdoc />
        public CsOptionMissingException(string? optionName = null)
            : base("value must be provided", optionName)
        {
        }


        /// <summary>
        /// Throws the CsOptionMissingException exception if the provided value is null.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="optionName">This parameter should normally not be passed, it is generated automatically from the value argument.</param>
        /// <exception cref="CsOptionMissingException"></exception>
        public static void ThrowIfNull([NotNull] object? value, [CallerArgumentExpression("value")] string? optionName = null)
        {
            if (value == null)
                throw new CsOptionMissingException(optionName);
        }


        /// <summary>
        /// Throws the CsOptionMissingException exception if the provided value is null or empty.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <param name="optionName">This parameter should normally not be passed, it is generated automatically from the value argument.</param>
        /// <exception cref="CsOptionMissingException"></exception>
        public static void ThrowIfEmpty([NotNull] string? value, [CallerArgumentExpression("value")] string? optionName = null)
        {
            if (string.IsNullOrEmpty(value))
                throw new CsOptionMissingException(optionName);
        }
    }
}
