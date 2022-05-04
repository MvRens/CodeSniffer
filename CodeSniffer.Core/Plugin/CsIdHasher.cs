using System.Security.Cryptography;
using System.Text;
using CodeSniffer.Core.Source;

namespace CodeSniffer.Core.Plugin
{
    /// <summary>
    /// Helper class for generating relatively short but unique and reproducable Id's
    /// for the likes of <see cref="ICsSourceCodeRepository"/>.
    /// </summary>
    /// <remarks>
    /// Yes, that's a roundabout way of saying it generates a SHA256.
    /// </remarks>
    public static class CsIdHasher
    {
        /// <summary>
        /// Helper method for generating relatively short but unique and reproducable Id's
        /// for the likes of <see cref="ICsSourceCodeRepository"/>.
        /// </summary>
        /// <remarks>
        /// Yes, that's a roundabout way of saying it generates a SHA256.
        /// </remarks>
        public static string Create(string input)
        {
            var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash);
        }
    }
}
