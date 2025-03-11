using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// The ViewModel for creating a stage
    /// </summary>
    public class CreateStageViewModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required(ErrorMessage = "Stage name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        [Required(ErrorMessage = "A position number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be a positive number.")]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the selected group identifier.
        /// </summary
        [Display(Name = "Assign Group")]
        public int? SelectedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the available groups.
        /// </summary>
        public IEnumerable<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();
    }
}