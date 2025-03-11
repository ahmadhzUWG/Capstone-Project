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
    options.AddPolicy("IsProjectLead", policy =>
        policy.Requirements.Add(new ProjectRoleRequirement("ProjectLead")));

    options.AddPolicy("IsAdmin", policy =>
        policy.Requirements.Add(new UserRoleRequirement("Admin")));

    options.AddPolicy("IsManager", policy =>
        policy.Requirements.Add(new ManagerRelationshipRequirement("Manager")));
});

builder.Services.AddScoped<IAuthorizationHandler, UserRoleHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ManagerRelationshipHandler>();
builder.Services.AddScoped<IAuthorizationHandler, GroupRoleHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ProjectRoleHandler>();

builder.Services.AddServerSideBlazor();

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

app.MapBlazorHub();

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
