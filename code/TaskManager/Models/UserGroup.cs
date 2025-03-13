using TaskManagerWebsite.Models;

/// <summary>
/// Represents the relationship between a user and a group.
/// </summary>
public class UserGroup
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    /// <value>
    /// The user identifier.
    /// </value>
    public int UserId { get; set; }
    /// <summary>
    /// Gets or sets the user.
    /// </summary>
    /// <value>
    /// The user.
    /// </value>
    public User User { get; set; }

    /// <summary>
    /// Gets or sets the group identifier.
    /// </summary>
    /// <value>
    /// The group identifier.
    /// </value>
    public int GroupId { get; set; }
    /// <summary>
    /// Gets or sets the group.
    /// </summary>
    /// <value>
    /// The group.
    /// </value>
    public Group Group { get; set; }

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    /// <value>
    /// The role.
    /// </value>
    public string Role { get; set; }
}