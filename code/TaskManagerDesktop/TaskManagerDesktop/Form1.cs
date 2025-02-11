using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using TaskManagerDesktop.Data;


namespace TaskManagerDesktop
{
    public partial class Form1 : Form
    {
        private readonly ApplicationDbContext _context;

        public Form1(ApplicationDbContext requiredService)
        {
            _context = requiredService ?? throw new ArgumentNullException(nameof(requiredService));
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
        public async Task<List<UserViewModel>> GetUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "N/A",
                    Email = u.Email ?? "N/A",
                    Role = u.Role ?? "No Role Assigned"
                })
                .ToListAsync();
        }

        public class UserViewModel
        {
            public int Id { get; set; }
            public string UserName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }

    }
}
