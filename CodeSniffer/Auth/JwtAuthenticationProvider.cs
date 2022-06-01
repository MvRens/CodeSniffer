using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CodeSniffer.Authentication;
using CodeSniffer.Repository.Users;
using JsonWebToken;

namespace CodeSniffer.Auth
{
    public class JwtAuthenticationProvider : IAuthenticationProvider
    {
        private readonly string issuer;
        private readonly string? audience;
        private readonly Jwk key;
        private readonly SignatureAlgorithm signatureAlgorithm;
        private readonly Func<IUserRepository> userRepositoryFactory;


        public JwtAuthenticationProvider(string issuer, string? audience, Jwk key, SignatureAlgorithm signatureAlgorithm, Func<IUserRepository> userRepositoryFactory)
        {
            this.issuer = issuer;
            this.audience = audience;
            this.key = key;
            this.signatureAlgorithm = signatureAlgorithm;
            this.userRepositoryFactory = userRepositoryFactory;
        }


        public async ValueTask<string?> Validate(string username, string password)
        {
            var userRepository = userRepositoryFactory();
            var user = await userRepository.ValidateLogin(username, password);

            if (user == null)
                return null;

            var descriptor = new JwsDescriptor(key, signatureAlgorithm)
            {
                Payload = new JwtPayload
                {
                    { JwtClaimNames.Iat, EpochTime.UtcNow },
                    { JwtClaimNames.Exp, EpochTime.UtcNow + EpochTime.OneHour },
                    { JwtClaimNames.Iss, issuer },
                    { JwtClaimNames.Sub, user.Username },
                    { CsJwtClaimNames.Name, user.DisplayName },
                    { CsJwtClaimNames.Role, user.Role }
                }
            };

            if (!string.IsNullOrEmpty(audience))
                descriptor.Payload.Add(JwtClaimNames.Aud, audience);

            var writer = new JwtWriter();
            return writer.WriteTokenString(descriptor);
        }
    }
}
