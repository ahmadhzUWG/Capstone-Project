using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Authorization;

public class UserRelationshipHandler : AuthorizationHandler<UserRelationshipRequirement, User>
{
    private readonly ApplicationDbContext _context;

    public UserRelationshipHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserRelationshipRequirement requirement, User targetUser)
    {
        // Convert userId from string to int
        if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            return; // Exit if conversion fails
        }

        // ✅ Check if the user is a "Manager" of the same group as the targetUser
        bool isManager = await _context.UserGroups
            .AnyAsync(ug => ug.UserId == userId && ug.Role == "Manager" &&
                            _context.UserGroups.Any(ug2 => ug2.UserId == targetUser.Id && ug2.GroupId == ug.GroupId));

        if (isManager)
        {
            context.Succeed(requirement);
        }
    }
}