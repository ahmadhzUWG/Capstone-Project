﻿using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.ViewModels;

/// <summary>
/// Represents the model used for creating a new group.
/// </summary>
public class GroupViewModel
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    [Required(ErrorMessage = "Group name is required. Please provide a unique and descriptive name for the group.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "Group name must be between 3 and 100 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    [Required(ErrorMessage = "Group description is required. Provide details about the group's purpose and objectives.")]
    [StringLength(255, MinimumLength = 10,
        ErrorMessage = "Description must be between 10 and 255 characters.")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the selected manager identifier.
    /// </summary>
    /// <value>
    /// The selected manager identifier.
    /// </value>
    [Required(ErrorMessage = "Please select a manager for this group. A group must have exactly one manager.")]
    public required int SelectedManagerId { get; set; }

    /// <summary>
    /// Gets or sets the selected user ids.
    /// </summary>
    /// <value>
    /// The selected user ids.
    /// </value>
    public List<int> SelectedUserIds { get; set; } = new List<int>();
}