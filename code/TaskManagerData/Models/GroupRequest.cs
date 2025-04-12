namespace TaskManagerData.Models;

/// <summary>
/// Represents a request made by a user to assign a group to a project
/// This request may be accepted or rejected.
/// </summary>
public class GroupRequest
{
    /// <summary>
    /// Gets or sets the unique identifier for the group request.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who sent the request.
    /// </summary>
    public int SenderId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the group to which the request is made.
    /// </summary>
    public int GroupId { get; set; }

    /// <summary>
    /// Gets or sets the group entity associated with the request.
    /// </summary>
    public Group Group { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the project that is being requested for association with the group.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the project entity associated with the request.
    /// </summary>
    public Project Project { get; set; }

    /// <summary>
    /// Gets or sets the response status of the request.
    /// A value of <c>true</c> indicates acceptance, <c>false</c> indicates rejection, and <c>null</c> means pending.
    /// </summary>
    public bool? Response { get; set; }
}