using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JsonWebToken;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSniffer.Auth
{
    public class JwtAuthenticationHandler : AuthenticationHandler<JwtAuthenticationOptions>
    {
        private const string AuthorizationHeader = @"Authorization";
        private const string AuthorizationBearerPrefix = @"Bearer ";


        public JwtAuthenticationHandler(IOptionsMonitor<JwtAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) 
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(AuthorizationHeader, out var authenticationHeader))
                return Task.FromResult(AuthenticateResult.NoResult());

            if (string.IsNullOrEmpty(authenticationHeader) || !authenticationHeader.ToString().StartsWith(AuthorizationBearerPrefix))
                return Task.FromResult(AuthenticateResult.NoResult());

            var authenticationToken = authenticationHeader.ToString()[AuthorizationBearerPrefix.Length..];
            var validationPolicy = Options.GetTokenValidationPolicy();

            Jwt? jwt = null;
            try
            {
                if (!Jwt.TryParse(authenticationToken, validationPolicy, out jwt))
                    return Task.FromResult(AuthenticateResult.Fail(jwt.Error?.ToString() ?? "Unauthorization"));

                var identity = new ClaimsIdentity(GetClaims(jwt), Scheme.Name);
                var principal = new GenericPrincipal(identity, Array.Empty<string>());
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            finally
            {
                jwt?.Dispose();
            }
        }


        private static IEnumerable<Claim> GetClaims(Jwt jwt)
        {
            if (jwt.Payload == null)
                yield break;

            if (jwt.Payload.TryGetClaim(JwtClaimNames.Sub, out var subject))
                yield return new Claim(ClaimTypes.Name, subject.GetString() ?? "");

            if (jwt.Payload.TryGetClaim(CsJwtClaimNames.Role, out var role))
                yield return new Claim(ClaimTypes.Role, role.GetString() ?? "");
        }
    }
}
