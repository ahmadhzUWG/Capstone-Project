using System.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagerData.Models;
using TaskManagerData.Services;
using TaskManagerData.Authorization;
using TaskManagerDesktop.Views;
using TaskManagerWebsite.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskManagerDesktop
{
    static class Program
    {
        [STAThread]
        static async Task Main()
        {
            var services = new ServiceCollection();

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthorization(options =>
            {
                // Admins can manage everything
                options.AddPolicy("CanManageUsers", policy =>
                    policy.Requirements.Add(new UserRoleRequirement("Admin")));

                // Managers can edit users they manage
                options.AddPolicy("CanEditUser", policy =>
                    policy.Requirements.Add(new UserRelationshipRequirement("Manager")));

                // Only Admins can delete users
                options.AddPolicy("CanDeleteUser", policy =>
                    policy.Requirements.Add(new UserRoleRequirement("Admin")));

                // Admins can create groups/projects
                options.AddPolicy("CanManageGroups", policy =>
                    policy.Requirements.Add(new UserRoleRequirement("Admin")));
                options.AddPolicy("CanManageProjects", policy =>
                    policy.Requirements.Add(new UserRoleRequirement("Admin")));

                // Managers can edit groups/projects they manage
                options.AddPolicy("CanEditGroup", policy =>
                    policy.Requirements.Add(new GroupRoleRequirement("Manager")));
                options.AddPolicy("CanEditProject", policy =>
                    policy.Requirements.Add(new ProjectRoleRequirement("Manager")));

                // Only Admins can delete groups/projects
                options.AddPolicy("CanDeleteGroup", policy =>
                    policy.Requirements.Add(new UserRoleRequirement("Admin")));
                options.AddPolicy("CanDeleteProject", policy =>
                    policy.Requirements.Add(new UserRoleRequirement("Admin")));

                // Managers can manage employees within their own group/project
                options.AddPolicy("CanManageGroupEmployees", policy =>
                    policy.Requirements.Add(new GroupRoleRequirement("Manager")));
                options.AddPolicy("CanManageProjectEmployees", policy =>
                    policy.Requirements.Add(new ProjectRoleRequirement("Manager")));
            });

            services.AddScoped<IAuthorizationHandler, UserRoleHandler>();
            services.AddScoped<IAuthorizationHandler, UserRelationshipHandler>();
            services.AddScoped<IAuthorizationHandler, GroupRoleHandler>();
            services.AddScoped<IAuthorizationHandler, ProjectRoleHandler>();

            services.AddSingleton<EmailService>();

            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();
            await DataSeeder.SeedRolesAndUsersAsync(serviceProvider);

            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                dbContext.Database.Migrate();
                Console.WriteLine("Database connection successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Login(serviceProvider));
        }
    }
}
