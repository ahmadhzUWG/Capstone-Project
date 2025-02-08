using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TaskManagerWebsite.Models
{
    public class UserMetadata
    {
        [StringLength(10, MinimumLength = 5)]
        public string Username;

        [StringLength(10, MinimumLength = 5)]
        public string Password;
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

}