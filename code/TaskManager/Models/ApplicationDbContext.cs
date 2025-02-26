using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Models;

namespace TaskManagerWebsite.Data;

/// <summary>
/// Represents the database context for the Task Manager Website application.
/// Inherits from <see cref="IdentityDbContext{TUser, TRole, TKey}"/> to support Identity features.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">Database context configuration options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Users table.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the Admins table.
    /// </summary>
    public DbSet<Admin> Admins { get; set; }

    /// <summary>
    /// Gets or sets the Groups table.
    /// </summary>
    public DbSet<Group> Groups { get; set; }

    /// <summary>
    /// Gets or sets the GroupManagers table.
    /// </summary>
    public DbSet<GroupManager> GroupManagers { get; set; }

    /// <summary>
    /// Gets or sets the Projects table.
    /// </summary>
    public DbSet<Project> Projects { get; set; }

    /// <summary>
    /// Gets or sets the GroupProjects table.
    /// </summary>
    public DbSet<GroupProject> GroupProjects { get; set; }

    /// <summary>
    /// Gets or sets the GroupRequests table.
    /// </summary>
    public DbSet<GroupRequest> GroupRequests { get; set; }

    /// <summary>
    /// Configures the entity relationships and database mappings.
    /// </summary>
    /// <param name="modelBuilder">Provides a simple API to configure entity relationships.</param>
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

        modelBuilder.Entity<GroupManager>()
            .HasKey(gm => new { gm.GroupId, gm.UserId });

        modelBuilder.Entity<GroupManager>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.Managers)
            .HasForeignKey(gm => gm.GroupId);

        modelBuilder.Entity<GroupManager>()
            .HasOne(gm => gm.User)
            .WithMany()
            .HasForeignKey(gm => gm.UserId);

        modelBuilder.Entity<Group>()
            .HasOne(g => g.PrimaryManager)
            .WithMany()
            .HasForeignKey(g => g.PrimaryManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many-to-Many Relationship: Projects <-> Groups
        modelBuilder.Entity<GroupProject>()
            .HasKey(gp => new { gp.ProjectId, gp.GroupId });

        modelBuilder.Entity<GroupProject>()
            .HasOne(gp => gp.Project)
            .WithMany(p => p.ProjectGroups)
            .HasForeignKey(gp => gp.ProjectId);

        modelBuilder.Entity<GroupProject>()
            .HasOne(gp => gp.Group)
            .WithMany(g => g.GroupProjects)
            .HasForeignKey(gp => gp.GroupId);
    }
}