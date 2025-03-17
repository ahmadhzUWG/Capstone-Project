using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Authorization;

public class ManagerRelationshipHandler : AuthorizationHandler<ManagerRelationshipRequirement, int>
{
    private readonly ApplicationDbContext _context;

    public ManagerRelationshipHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ManagerRelationshipRequirement requirement, int groupId)
    {
        if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return;
        }

        var group = await _context.Groups.FindAsync(groupId);
        if (group == null)
        {
            return;
        }

        if (group.ManagerId == currentUserId)
        {
            context.Succeed(requirement);
        }
    }
}
