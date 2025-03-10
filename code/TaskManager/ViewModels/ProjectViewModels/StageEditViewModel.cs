using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// The viewModel for editing a stage.
    /// </summary>
    public class StageEditViewModel
    {
        /// <summary>
        /// Gets or sets the stage identifier.
        /// </summary>
        public int StageId { get; set; }

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required(ErrorMessage = "Stage name is required.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        [Required(ErrorMessage = "A position is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be a positive number.")]
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets the selected group identifier.
        /// </summary>
   
        [Display(Name = "Assign Group")]
        public int? SelectedGroupId { get; set; }

        /// <summary>
        /// Gets or sets the available groups.
        /// </summary>
        public List<SelectListItem> AvailableGroups { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Gets or sets all stages.
        /// </summary>
        public List<Stage> AllStages { get; set; } = new List<Stage>();

    }
}