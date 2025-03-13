using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Authorization;

/// <summary>
/// Represents a handler for the <see cref="UserRelationshipRequirement"/>.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authorization.AuthorizationHandler&lt;TaskManagerWebsite.Authorization.UserRelationshipRequirement, TaskManagerWebsite.Models.User&gt;" />
public class UserRelationshipHandler : AuthorizationHandler<UserRelationshipRequirement, User>
{
    /// <summary>
    /// The context
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRelationshipHandler"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public UserRelationshipHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the requirement asynchronous.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="requirement">The requirement.</param>
    /// <param name="targetUser">The target user.</param>
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