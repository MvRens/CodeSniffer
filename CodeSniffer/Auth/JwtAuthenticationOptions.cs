using JsonWebToken;
using Microsoft.AspNetCore.Authentication;

namespace CodeSniffer.Auth
{
    public class JwtAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public Jwk? Key { get; set; }
        public SignatureAlgorithm SignatureAlgorithm { get; set; } = SignatureAlgorithm.HS256;



        public virtual TokenValidationPolicy GetTokenValidationPolicy()
        {
            var builder = new TokenValidationPolicyBuilder();

            if (!string.IsNullOrEmpty(Issuer) && Key != null)
                builder.RequireSignature(Issuer, Key, SignatureAlgorithm);

            if (!string.IsNullOrEmpty(Audience))
                builder.RequireAudience(Audience);

            return builder.Build();
        }
    }
}
