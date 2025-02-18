using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskManagerDesktop.Models
{
    public class User : IdentityUser<int>
    {
        public string? Role { get; set; }
    }
}