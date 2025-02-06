using System.Data.Entity;

namespace TaskManagerWebsite.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("Data Source=(localdb)\\ProjectModels;Initial Catalog=TaskManagerDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User", "dbo");
            base.OnModelCreating(modelBuilder);
        }


        public DbSet<User> Users { get; set; }
    }
}