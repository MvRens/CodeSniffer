namespace CodeSniffer.Authentication
{
    public interface IAuthenticationProvider
    {
        ValueTask<string?> Validate(string username, string password);
    }
}