using Microsoft.AspNetCore.Authorization;

namespace TaskManagerData.Authorization
{
    /// <summary>
    /// Represents a requirement for a user role.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authorization.IAuthorizationRequirement" />
    public class UserRoleRequirement(string requiredRole) : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the required role.
        /// </summary>
        /// <value>
        /// The required role.
        /// </value>
        public string RequiredRole { get; } = requiredRole;
    }
}
