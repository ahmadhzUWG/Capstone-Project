using Microsoft.AspNetCore.Authorization;

namespace TaskManagerWebsite.Authorization;

public class ManagerRelationshipRequirement(string requiredRelationship) : IAuthorizationRequirement
{
    /// <summary>
    /// Represents a requirement for a user relationship.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authorization.IAuthorizationRequirement" />
    public string RequiredRelationship { get; } = requiredRelationship;
}