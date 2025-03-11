using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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
        // Get the logged-in user's ID from claims
        if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return;
        }

        // Retrieve the group from the database using the groupId
        var group = await _context.Groups.FindAsync(groupId);
        if (group == null)
        {
            return;
        }

        // If the logged-in user is the manager of this group, succeed the requirement
        if (group.ManagerId == currentUserId)
        {
            context.Succeed(requirement);
        }
    }
}
