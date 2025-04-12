using Microsoft.AspNetCore.Authorization;

namespace TaskManagerData.Authorization;

/// <summary>
/// Represents a requirement for a project role.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authorization.IAuthorizationRequirement" />
public class ProjectRoleRequirement(string requiredRole) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the required role.
    /// </summary>
    /// <value>
    /// The required role.
    /// </value>
    public string RequiredRole { get; } = requiredRole;
}