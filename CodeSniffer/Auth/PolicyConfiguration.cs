using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace CodeSniffer.Auth
{
    public static class PolicyConfiguration
    {
        public static void Apply(AuthorizationOptions options)
        {
            options.LinkRoles(CsPolicyNames.Developers, CsRoleNames.Developer);
            options.LinkRoles(CsPolicyNames.Admins, CsRoleNames.Developer, CsRoleNames.Admin);
        }


        private static void LinkRoles(this AuthorizationOptions options, string policy, params string[] roles)
        {
            options.AddPolicy(policy, p => p.AddRequirements(new RolesAuthorizationRequirement(roles)));
        }
    }
}
