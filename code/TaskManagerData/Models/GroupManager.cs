namespace TaskManagerData.Models;

/// <summary>
/// Represents the relationship between a user and a group as a manager.
/// A group manager is responsible for overseeing a specific group.
/// </summary>
public class GroupManager
{
    /// <summary>
    /// Gets or sets the identifier of the group that the manager is associated with.
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Gets or sets the group entity associated with this manager.
    /// </summary>
    public Group Group { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who is assigned as a group manager.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user entity who is managing the group.
    /// </summary>
    public User User { get; set; }
}