using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Data.SqlClient.DataClassification;
using System.ComponentModel.DataAnnotations;

namespace TaskManagerWebsite.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Project Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        public int ProjectLeadId { get; set; }
        [ValidateNever]
        public User ProjectLead { get; set; }

        public int ProjectCreatorId { get; set; }

        public ICollection<Group> ProjectGroups { get; set; } = new List<Group>();
    }
    }
