﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManagerData.Models;

namespace TaskManagerWebsite.ViewModels.ProjectViewModels
{
    /// <summary>
    /// The ViewModel for creating a task
    /// </summary>
    public class CreateTaskViewModel
    {

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the selected stage identifier.
        /// </summary>
        [Display(Name = "Assign Employee")]
        public int? SelectedEmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the available employees.
        /// </summary>
        public IEnumerable<SelectListItem> AvailableEmployees { get; set; } = new List<SelectListItem>();

        /// <summary>
        /// Gets or sets the stage identifier.
        /// </summary>
        public int StageId { get; set; }

        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets the task history.
        /// </summary>
        public List<TaskHistory> TaskHistory { get; set; } = new List<TaskHistory>();

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        public List<Comment>? Comments { get; set; }

        /// <summary>
        /// Creates new comment.
        /// </summary>
        /// <value>
        /// The new comment.
        /// </value>
        public string? NewComment { get; set; }

    }
}
