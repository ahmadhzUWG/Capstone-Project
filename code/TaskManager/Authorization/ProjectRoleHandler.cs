using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskManagerWebsite.Authorization;
using TaskManagerWebsite.Data;

namespace TaskManagerWebsite.Authorization;

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

        bool isProjectManager = await _context.UserGroups
            .Where(ug => ug.UserId == userId && ug.Role == "Manager")
            .AnyAsync(ug => _context.GroupProjects
                .Any(gp => gp.GroupId == ug.GroupId && gp.ProjectId == projectId));

        if (isProjectManager)
        {
            context.Succeed(requirement);
        }
    }
}