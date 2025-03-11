using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Authorization;
using TaskManagerWebsite.Data;

public class ProjectRoleHandler : AuthorizationHandler<ProjectRoleRequirement, int>
{
    private readonly ApplicationDbContext _context;

    public ProjectRoleHandler(ApplicationDbContext context)
    {
        _context = context;
    }

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