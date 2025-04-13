using Microsoft.AspNetCore.Authorization;

namespace TaskManagerData.Authorization;

/// <summary>
/// Represents a requirement for a user relationship.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Authorization.IAuthorizationRequirement" />
public class UserRelationshipRequirement(string requiredRelationship) : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the required relationship.
    /// </summary>
    /// <value>
    /// The required relationship.
    /// </value>
    public string RequiredRelationship { get; } = requiredRelationship;
}