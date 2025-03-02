using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Authorization;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = false;
    options.LoginPath = "/Login/Index"; 
    options.LogoutPath = "/Login/Index";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

builder.Services.AddAuthorization(options =>
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

builder.Services.AddScoped<IAuthorizationHandler, UserRoleHandler>();
builder.Services.AddScoped<IAuthorizationHandler, UserRelationshipHandler>();
builder.Services.AddScoped<IAuthorizationHandler, GroupRoleHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectRoleHandler>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataSeeder.SeedRolesAndUsersAsync(services);

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("Database connection successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shared/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled Exception: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}
