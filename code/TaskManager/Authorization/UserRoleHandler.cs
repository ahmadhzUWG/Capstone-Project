using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerWebsite.Authorization;

namespace TaskManagerWebsite.Authorization;

public class UserRoleHandler : AuthorizationHandler<UserRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRoleRequirement requirement)
    {
        var userRole = context.User.FindFirstValue(ClaimTypes.Role);

        if (userRole == requirement.RequiredRole)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}