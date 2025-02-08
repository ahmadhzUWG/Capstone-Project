using System.Windows.Forms;
using TaskManagerWebsite.Models;

namespace TaskManagerDesktop
{
    public partial class Form1 : Form
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public Form1()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                var products = _context.Users.ToList();
                dataGridView1.DataSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
