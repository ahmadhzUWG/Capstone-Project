using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;
using TaskManagerWebsite.Models;

namespace TaskManagerDesktop
{
    public partial class Form1 : Form
    {
        private readonly ApplicationDbContext _context = new();

        public Form1(ApplicationDbContext requiredService)
        {
            InitializeComponent();
            LoadUsers();
        }

        private async void LoadUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? "N/A",
                        Email = u.Email ?? "N/A",
                        Role = u.Role ?? "No Role Assigned"
                    })
                    .ToListAsync();

                dataGridView1.DataSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

    }
}
