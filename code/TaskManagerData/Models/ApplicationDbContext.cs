using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TaskManagerData.Models;

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

    public DbSet<UserGroup> UserGroups { get; set; }

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
    /// Gets or sets the project boards.
    /// </summary>
    public DbSet<ProjectBoard> ProjectBoards { get; set; }

    /// <summary>
    /// Gets or sets the stages.
    /// </summary>
    public DbSet<Stage> Stages { get; set; }

    /// <summary>
    /// Gets or sets the tasks.
    /// </summary>
    public DbSet<Models.Task> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the task stages.
    /// </summary>
    public DbSet<TaskStage> TaskStages { get; set; }

    /// <summary>
    /// Gets or sets the task employees.
    /// </summary>
    public DbSet<TaskEmployee> TaskEmployees { get; set; }

    /// <summary>
    /// Gets or sets the task histories.
    /// </summary>
    public DbSet<TaskHistory> TaskHistories { get; set; }

    /// <summary>
    /// Gets or sets the comments.
    /// </summary>
    public DbSet<Comment> Comments { get; set; }

    /// <summary>
    /// Gets or sets the password resets.
    /// </summary>
    public DbSet<PasswordReset> PasswordResets { get; set; }


    /// <summary>
    /// Configures the entity relationships and database mappings.
    /// </summary>
    /// <param name="modelBuilder">Provides a simple API to configure entity relationships.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Many-to-Many Relationship: Users <-> Groups (Explicitly Define UserGroup)
        modelBuilder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(ug => ug.UserId);

        modelBuilder.Entity<UserGroup>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(ug => ug.GroupId);


        // One-to-One: Admin <-> User
        modelBuilder.Entity<Admin>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupManager>()
            .HasKey(gm => new { gm.UserId, gm.GroupId });

        modelBuilder.Entity<GroupManager>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.Managers)
            .HasForeignKey(gm => gm.GroupId);

        modelBuilder.Entity<GroupManager>()
            .HasOne(gm => gm.User)
            .WithMany()
            .HasForeignKey(gm => gm.UserId);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.ProjectLead)
            .WithMany()
            .HasForeignKey(p => p.ProjectLeadId)
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

        // Board -> Stages (One-to-Many)
        modelBuilder.Entity<Stage>()
            .HasOne(s => s.ProjectBoard)
            .WithMany(b => b.Stages)
            .HasForeignKey(s => s.ProjectBoardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Stage>()
            .HasOne(s => s.CreatorGroup)
            .WithMany()
            .HasForeignKey(s => s.CreatorGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Stage>()
            .HasOne(s => s.CreatorUser)
            .WithMany()
            .HasForeignKey(s => s.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Stage>()
            .HasOne(s => s.AssignedGroup)
            .WithMany()
            .HasForeignKey(s => s.AssignedGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // ProjectBoard -> User (One-to-Many)
        modelBuilder.Entity<ProjectBoard>()
            .HasOne(pb => pb.BoardCreator)
            .WithMany()
            .HasForeignKey(pb => pb.BoardCreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Project>()
            .HasOne(p => p.ProjectBoard)
            .WithOne(b => b.Project)
            .HasForeignKey<ProjectBoard>(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> TaskEmployee (One-to-Many)
        modelBuilder.Entity<TaskEmployee>()
            .HasOne(te => te.Task)
            .WithMany(t => t.TaskEmployees)
            .HasForeignKey(te => te.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> TaskEmployee (One-to-Many)
        modelBuilder.Entity<TaskEmployee>()
            .HasOne(te => te.Employee)
            .WithMany(e => e.TaskEmployees)
            .HasForeignKey(te => te.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Task -> TaskStage (One-to-Many)
        modelBuilder.Entity<TaskStage>()
            .HasOne(ts => ts.Task)
            .WithMany(t => t.TaskStages)
            .HasForeignKey(ts => ts.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Stage -> TaskStage (One-to-Many)
        modelBuilder.Entity<TaskStage>()
            .HasOne(ts => ts.Stage)
            .WithMany(s => s.TaskStages)
            .HasForeignKey(ts => ts.StageId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TaskHistory>()
            .HasOne(th => th.Task)
            .WithMany()
            .HasForeignKey(th => th.TaskId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskHistory>()
            .HasOne(th => th.User)
            .WithMany()
            .HasForeignKey(th => th.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}