using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

var builder = WebApplication.CreateBuilder(args);

//  Configure Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  Configure Identity with Integer User IDs
builder.Services.AddIdentity<User, IdentityRole<int>>() // If using IdentityRole<int>, ensure it's correctly configured in ApplicationDbContext
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

//  Configure Authentication Cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Redirect to login page if unauthorized
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(async context =>
        {
            var httpContext = context.Resource as HttpContext;
            if (httpContext == null) return false;

            var userManager = httpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var dbContext = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var user = await userManager.GetUserAsync(httpContext.User);
            return user != null && await dbContext.Admins.AnyAsync(a => a.UserId == user.Id);
        }));
});

//  Add services to the container
builder.Services.AddControllersWithViews();

var app = builder.Build();

//  Ensure the Database is Migrated BEFORE the App Runs
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        dbContext.Database.Migrate(); // Applies pending migrations
        Console.WriteLine("Database connection successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
}

//  Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//  Set default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled Exception: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}
