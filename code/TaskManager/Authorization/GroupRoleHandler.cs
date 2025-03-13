using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerWebsite.Authorization;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Authorization;

/// <summary>
/// Represents a handler for the <see cref="GroupRoleRequirement"/>.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authorization.AuthorizationHandler&lt;TaskManagerWebsite.Authorization.GroupRoleRequirement, System.Int32&gt;" />
public class GroupRoleHandler : AuthorizationHandler<GroupRoleRequirement, int>
{
    /// <summary>
    /// The context
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupRoleHandler"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public GroupRoleHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Handles the requirement asynchronous.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="requirement">The requirement.</param>
    /// <param name="groupId">The group identifier.</param>
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