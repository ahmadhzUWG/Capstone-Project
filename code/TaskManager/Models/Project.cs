using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.Models;

/// <summary>
/// Represents a project within the task management system.
/// A project is led by a user and can be associated with multiple groups.
/// </summary>
public class Project
{
    /// <summary>
    /// Gets or sets the unique identifier for the project.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the project.
    /// This field is required and must not exceed 100 characters.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Project Name cannot exceed 100 characters")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the project.
    /// This field is required and must not exceed 500 characters.
    /// </summary>
    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the project lead.
    /// The project lead is responsible for managing the project.
    /// </summary>
    public int ProjectLeadId { get; set; }

    /// <summary>
    /// Gets or sets the user entity who leads the project.
    /// This property is not validated during model binding.
    /// </summary>
    [ValidateNever]
    public User ProjectLead { get; set; }

    /// <summary>
    /// Gets or sets the optional user ID of the project creator.
    /// The creator is the user who originally initiated the project.
    /// </summary>
    public int? ProjectCreatorId { get; set; }

    /// <summary>
    /// Gets or sets the collection of group associations linked to this project.
    /// A project can be associated with multiple groups.
    /// </summary>
    public ICollection<GroupProject> ProjectGroups { get; set; } = new List<GroupProject>();
}