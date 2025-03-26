using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Authorization
{
    public class AdminRoleHandler(ApplicationDbContext context) : AuthorizationHandler<AdminRoleRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context1, AdminRoleRequirement requirement)
        {
            if (!int.TryParse(context1.User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
            {
                return;
            }

            var userRole = await context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(
                    context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name
                )
                .FirstOrDefaultAsync();

            if (userRole == requirement.RequiredRole)
            {
                context1.Succeed(requirement);
            }
        }
    }
}