using Microsoft.AspNetCore.Authorization;

namespace TaskManagerWebsite.Authorization;

public class GroupRoleRequirement(string requiredRole) : IAuthorizationRequirement
{
    public string RequiredRole { get; } = requiredRole;
}