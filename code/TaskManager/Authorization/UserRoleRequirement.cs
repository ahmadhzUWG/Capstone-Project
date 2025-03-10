namespace TaskManagerWebsite.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    public class UserRoleRequirement(string requiredRole) : IAuthorizationRequirement
    {
        public string RequiredRole { get; } = requiredRole;
    }
}
