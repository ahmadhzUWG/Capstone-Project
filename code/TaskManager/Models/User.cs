using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace TaskManagerWebsite.Models;

public class User : IdentityUser<int>
{
    public ICollection<Group> Groups { get; set; } = new List<Group>();
}