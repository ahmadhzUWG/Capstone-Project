using Microsoft.AspNetCore.Identity;

namespace TaskManagerData.Models;

/// <summary>
/// Represents a user in the task management system.
/// Inherits from <see cref="IdentityUser{TKey}" /> to integrate with ASP.NET Identity.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Identity.IdentityUser&lt;System.Int32&gt;" />
public class User : IdentityUser<int>
{
    /// <summary>
    /// Gets or sets the collection of groups the user is a member of.
    /// A user can belong to multiple groups.
    /// </summary>
    /// <value>
    /// The user groups.
    /// </value>
    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    /// <summary>
    /// Gets or sets the collection of tasks the user is assigned to.
    /// </summary>
    public ICollection<TaskEmployee> TaskEmployees { get; set; } = new List<TaskEmployee>();
}