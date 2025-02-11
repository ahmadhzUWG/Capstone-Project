using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TaskManagerDesktop.Data;

using Microsoft.Extensions.Configuration;
using TaskManagerDesktop.Models;

namespace TaskManagerDesktop.Services
{
    public class IdentityService(UserManager<User> userManager)
    {
        private readonly UserManager<User> _userManager = userManager;

        public async Task<IdentityResult> RegisterUserAsync(User user, string password)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName) || string.IsNullOrEmpty(user.Email))
            {
                throw new ArgumentException("User, UserName, and Email are required.");
            }

            var existingUser = await _userManager.FindByNameAsync(user.UserName);
            if (existingUser != null)
            {
                throw new Exception($"User {user.UserName} already exists.");
            }

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new Exception($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return result;
        }


        public static ServiceProvider ConfigureServices(bool useInMemoryDb = false)
        {
            var services = new ServiceCollection();

            if (useInMemoryDb)
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer("Data Source=(localdb)\\ProjectModels;Initial Catalog=TaskManagerDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;"));
            }

            services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IdentityService>();

            services.AddLogging();

            return services.BuildServiceProvider();
        }

    }
}