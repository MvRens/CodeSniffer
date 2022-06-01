using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CodeSniffer.Auth
{
    public class JwtAuthorizationHandler : AuthorizationHandler<JwtAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, JwtAuthorizationRequirement requirement)
        {
            if (context.User.Identity is not { IsAuthenticated: true })
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
