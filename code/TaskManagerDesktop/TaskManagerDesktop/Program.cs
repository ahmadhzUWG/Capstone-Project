using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols;
using TaskManagerDesktop;
using TaskManagerDesktop.Models;
using TaskManagerDesktop.Data;

namespace WinFormsApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            var services = new ServiceCollection();

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var serviceProvider = services.BuildServiceProvider();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            dbContext.Database.EnsureCreated();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(serviceProvider.GetRequiredService<ApplicationDbContext>()));
        }
    }
}
