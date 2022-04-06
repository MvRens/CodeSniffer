using System.Diagnostics.CodeAnalysis;

namespace CodeSniffer.Authentication
{
    public interface IAuthenticationProvider
    {
        bool Validate(string username, string password, [NotNullWhen(true)]out string? token);
    }
}