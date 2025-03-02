using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.ViewModels;

public class GroupViewModel
{
    [Required(ErrorMessage = "Group name is required. Please provide a unique and descriptive name for the group.")]
    [StringLength(100, ErrorMessage = "Group name cannot exceed 100 characters. Please use a shorter name.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Group description is required. Provide details about the group's purpose and objectives.")]
    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters. Try summarizing the information.")]
    public string Description { get; set; }

    [Required(ErrorMessage = "Please select a manager for this group. A group must have exactly one manager.")]
    public int? SelectedManagerId { get; set; }

    public List<int> SelectedUserIds { get; set; } = new List<int>();
}