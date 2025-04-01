using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// The ViewModel for editing a task
    /// </summary>
    public class EditTaskViewModel
    {
        public int TaskId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public int? SelectedEmployeeId { get; set; }

        public List<SelectListItem> AvailableEmployees { get; set; } = new();
    }
}
