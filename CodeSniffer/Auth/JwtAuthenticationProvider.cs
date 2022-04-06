using System.Diagnostics.CodeAnalysis;
using CodeSniffer.Authentication;
using JsonWebToken;

namespace CodeSniffer.Auth
{
    public class JwtAuthenticationProvider : IAuthenticationProvider
    {
        private readonly string issuer;
        private readonly string? audience;
        private readonly Jwk key;
        private readonly SignatureAlgorithm signatureAlgorithm;


        public JwtAuthenticationProvider(string issuer, string? audience, Jwk key, SignatureAlgorithm signatureAlgorithm)
        {
            this.issuer = issuer;
            this.audience = audience;
            this.key = key;
            this.signatureAlgorithm = signatureAlgorithm;
        }


        public bool Validate(string username, string password, [NotNullWhen(true)]out string? token)
        {
            // TODO check user in db
            if (username != "admin")
            {
                token = null;
                return false;
            }

            var descriptor = new JwsDescriptor(key, signatureAlgorithm)
            {
                Payload = new JwtPayload
                {
                    { JwtClaimNames.Iat, EpochTime.UtcNow },
                    { JwtClaimNames.Exp, EpochTime.UtcNow + EpochTime.OneHour },
                    { JwtClaimNames.Iss, issuer },
                    { JwtClaimNames.Sub, username },
                    { CsJwtClaimNames.Role, CsRoleNames.Admin }
                }
            };

            if (!string.IsNullOrEmpty(audience))
                descriptor.Payload.Add(JwtClaimNames.Aud, audience);

            var writer = new JwtWriter();
            token = writer.WriteTokenString(descriptor);
            return true;
        }
    }
}
