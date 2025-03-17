using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Authorization;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Authorization;

/// <summary>
/// Represents a handler for the <see cref="ProjectRoleRequirement"/>.
/// </summary>
/// <seealso cref="int" />
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
        _context = context;
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

        bool isProjectLead = await _context.Projects
            .Where(p => p.Id == projectId)
            .AnyAsync(p => p.ProjectLeadId == userId);

        if (isProjectLead)
        {
            context.Succeed(requirement);
        }
    }
}