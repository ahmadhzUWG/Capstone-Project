using Microsoft.AspNetCore.Authorization;

namespace TaskManagerWebsite.Authorization;

public class ProjectRoleRequirement(string requiredRole) : IAuthorizationRequirement
{
    public string RequiredRole { get; } = requiredRole;
}