using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels;

public class ProjectsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public List<Project> Projects { get; set; }
    public List<GroupRequest>? GroupRequests { get; set; }
    public List<GroupRequest>? SentGroupRequests { get; set; }

}