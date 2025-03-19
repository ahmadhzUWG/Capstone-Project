using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.ViewModels;

public class GroupsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SuccessMessage { get; set; }
    public string ErrorMessage { get; set; }
    public List<Group> Groups { get; set; }
}