using Microsoft.AspNetCore.Identity;

namespace TaskManagerWebsite.Models;

/// <summary>
/// Represents a user in the task management system.
/// Inherits from <see cref="IdentityUser{TKey}"/> to integrate with ASP.NET Identity.
/// </summary>
public class User : IdentityUser<int>
{
    /// <summary>
    /// Gets or sets the collection of groups the user is a member of.
    /// A user can belong to multiple groups.
    /// </summary>
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}