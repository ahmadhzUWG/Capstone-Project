using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels;

/// <summary>
/// 
/// </summary>
public class GroupsViewModel
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    /// <value>
    /// The identifier.
    /// </value>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the success message.
    /// </summary>
    /// <value>
    /// The success message.
    /// </value>
    public string? SuccessMessage { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    /// <value>
    /// The error message.
    /// </value>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the groups.
    /// </summary>
    /// <value>
    /// The groups.
    /// </value>
    public List<Group>? Groups { get; set; }

    /// <summary>
    /// Gets the undeleted group identifier.
    /// </summary>
    /// <value>
    /// The undeleted group identifier.
    /// </value>
    public int UndeletedGroupId { get; set; }

    public List<Project> RelatedProjects { get; set; }
}