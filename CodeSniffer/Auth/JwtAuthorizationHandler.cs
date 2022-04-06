using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CodeSniffer.Auth
{
    // TODO is this even needed? we can probably use one of the standard implementations since we're not checking any additional requirements here
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
