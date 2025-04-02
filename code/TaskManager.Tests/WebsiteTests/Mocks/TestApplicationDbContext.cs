using Microsoft.EntityFrameworkCore;
using TaskManagerWebsite.Data;

public class TestApplicationDbContext : ApplicationDbContext
{
    // When true, SaveChangesAsync will throw a DbUpdateException.
    public bool ThrowOnSaveChanges { get; set; } = false;

    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnSaveChanges)
        {
            throw new DbUpdateException();
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}