using Microsoft.AspNetCore.Authorization;

namespace TaskManagerWebsite.Authorization;

public class UserRelationshipRequirement(string requiredRelationship) : IAuthorizationRequirement
{
    public string RequiredRelationship { get; } = requiredRelationship;
}