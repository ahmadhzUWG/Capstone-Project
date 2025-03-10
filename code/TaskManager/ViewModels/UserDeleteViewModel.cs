using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels
{
    public class UserDeleteViewModel
    {
        public User User { get; set; }
        public List<Group> RelatedGroups { get; set; }
        public List<Project> RelatedProjects { get; set; }
    }
}
