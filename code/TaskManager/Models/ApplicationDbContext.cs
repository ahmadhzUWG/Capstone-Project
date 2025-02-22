using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupManager> GroupManagers { get; set; }

    public DbSet<Project> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many Relationship: Users <-> Groups (General Membership)
        modelBuilder.Entity<User>()
            .HasMany(u => u.Groups)
            .WithMany(g => g.Users)
            .UsingEntity<Dictionary<string, object>>(
                "UserGroup",
                j => j.HasOne<Group>().WithMany().HasForeignKey("GroupId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                j =>
                {
                    j.HasKey("UserId", "GroupId");
                    j.ToTable("UserGroups");
                });

        // One-to-One: Admin <-> User
        modelBuilder.Entity<Admin>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-Many Relationship: Group <-> Managers (with Primary Manager field)
        modelBuilder.Entity<GroupManager>()
            .HasKey(gm => new { gm.GroupId, gm.UserId });

        modelBuilder.Entity<GroupManager>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.Managers)
            .HasForeignKey(gm => gm.GroupId);

        modelBuilder.Entity<GroupManager>()
            .HasOne(gm => gm.User)
            .WithMany(u => u.ManagedGroups)
            .HasForeignKey(gm => gm.UserId);

    }
}
