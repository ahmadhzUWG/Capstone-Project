using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagerData.Authorization;

/// <summary>
/// Represents a handler for the <see cref="UserRoleRequirement"/>.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authorization.AuthorizationHandler&lt;TaskManagerWebsite.Authorization.UserRoleRequirement&gt;" />
public class UserRoleHandler : AuthorizationHandler<UserRoleRequirement>
{
    /// <summary>
    /// Makes a decision if authorization is allowed based on a specific requirement.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns></returns>
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