using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerWebsite.Authorization;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Authorization;

/// <summary>
/// Represents a handler for the <see cref="ProjectRoleRequirement"/>.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authorization.AuthorizationHandler&lt;TaskManagerWebsite.Authorization.ProjectRoleRequirement, System.Int32&gt;" />
public class ProjectRoleHandler : AuthorizationHandler<ProjectRoleRequirement, int>
{
    /// <summary>
    /// The context
    /// </summary>
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRoleHandler"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public ProjectRoleHandler(ApplicationDbContext context)
    {
        this._context = context;
    }

    /// <summary>
    /// Handles the requirement asynchronous.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="requirement">The requirement.</param>
    /// <param name="projectId">The project identifier.</param>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectRoleRequirement requirement, int projectId)
    {
        if (!int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            return;
        }

        bool isProjectManager = await this._context.UserGroups
            .Where(ug => ug.UserId == userId && ug.Role == "Manager")
            .AnyAsync(ug => this._context.GroupProjects
                .Any(gp => gp.GroupId == ug.GroupId && gp.ProjectId == projectId));

        if (isProjectManager)
        {
            context.Succeed(requirement);
        }
    }
}