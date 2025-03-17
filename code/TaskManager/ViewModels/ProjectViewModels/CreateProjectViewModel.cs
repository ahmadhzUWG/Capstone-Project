using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// ViewModel used for creating a new project.
    /// Contains validation rules and any data needed for the form.
    /// </summary>
    public class CreateProjectViewModel
    {
        /// <summary>
        /// Name of the project.
        /// </summary>
        [Required(ErrorMessage = "Project name is required. Please provide a brief, descriptive name.")]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Project name must be between 3 and 100 characters.")]
        public string Name { get; set; }

        /// <summary>
        /// Description or overview of the project.
        /// </summary>
        [Required(ErrorMessage = "A project description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters. Please summarize succinctly.")]
        public string Description { get; set; }

        /// <summary>
        /// The user ID of the project lead.
        /// </summary>
        [Required(ErrorMessage = "Please select a project lead.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project lead.")]
        public int ProjectLeadId { get; set; }

        /// <summary>
        /// Drop-down list items for potential project leads.
        /// </summary>
        public List<SelectListItem> ProjectLeads { get; set; } = new();
    }
}