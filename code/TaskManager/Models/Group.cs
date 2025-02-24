using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.SignalR;

namespace TaskManagerWebsite.Models;

public class Group
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Group Name cannot exceed 100 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; }

    public ICollection<User> Users { get; set; } = [];

    public ICollection<GroupManager> Managers { get; set; } = [];
    
    public int? PrimaryManagerId { get; set; }

    [ValidateNever]
    public User? PrimaryManager { get; set; }

    public ICollection<GroupProject> GroupProjects { get; set; } = [];
}

