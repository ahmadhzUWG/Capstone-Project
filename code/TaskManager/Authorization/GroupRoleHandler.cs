using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerWebsite.Authorization;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Authorization;

public class GroupRoleHandler : AuthorizationHandler<GroupRoleRequirement, int>
{
    private readonly ApplicationDbContext _context;

    public GroupRoleHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupRoleRequirement requirement, int groupId)
    {
        if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            return;
        }

        bool isGroupManager = await _context.UserGroups
            .AnyAsync(ug => ug.UserId == userId && ug.GroupId == groupId && ug.Role == "Manager");

        if (isGroupManager)
        {
            context.Succeed(requirement);
        }
    }
}