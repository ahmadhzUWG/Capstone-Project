using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;
using TaskManagerDesktop.Models;

namespace TaskManagerDesktop.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        // Constructor that accepts DbContextOptions (needed by EF Core)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        ////// ADD THIS PARAMETERLESS CONSTRUCTOR
        //public ApplicationDbContext() : base(new DbContextOptionsBuilder<ApplicationDbContext>()
        //    .UseSqlServer(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString)
        //    .Options)
        //    {
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}