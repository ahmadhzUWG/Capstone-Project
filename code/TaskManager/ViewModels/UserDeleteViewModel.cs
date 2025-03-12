using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels
{
    public class UserDeleteViewModel
    {
        public required User User { get; set; }
        public required List<Group> RelatedGroups { get; set; }
        public required List<Project> RelatedProjects { get; set; }
    }
}
