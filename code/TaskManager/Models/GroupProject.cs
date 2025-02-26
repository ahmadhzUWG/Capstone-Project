namespace TaskManagerWebsite.Models;

/// <summary>
/// Represents the association between a project and a group.
/// A project can be linked to multiple groups, and a group can have multiple projects.
/// </summary>
public class GroupProject
{
    /// <summary>
    /// Gets or sets the identifier of the project associated with the group.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project entity associated with the group.
    /// </summary>
    public Project Project { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the group associated with the project.
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Gets or sets the group entity associated with the project.
    /// </summary>
    public Group Group { get; set; }
}