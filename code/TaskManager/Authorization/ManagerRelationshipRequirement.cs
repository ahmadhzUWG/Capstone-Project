using Microsoft.AspNetCore.Authorization;

namespace TaskManagerWebsite.Authorization;

public class ManagerRelationshipRequirement(string requiredRelationship) : IAuthorizationRequirement
{
    public string RequiredRelationship { get; } = requiredRelationship;
}