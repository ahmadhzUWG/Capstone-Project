using System.ComponentModel.DataAnnotations;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels
{
    /// <summary>
    /// Represents the data model for creating an admin user.
    /// </summary>
    public class UsersViewModel
    {
        public List<User> Users { get; set; }

        public string SuccessMessage { get; set; }

        public string ErrorMessage { get; set; }

        public int UndeletedUserId { get; set; }

        public List<Group> RelatedGroups { get; set; }

        public List<Project?> RelatedProjects { get; set; }
    }
}