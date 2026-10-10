using Microsoft.EntityFrameworkCore;
using Ploch.Data.EFCore;
using Ploch.Data.Model;
using Ploch.TemplateApp.Model;

namespace Ploch.TemplateApp.Data;

public class TemplateAppDbContext : DbContext
{
    private readonly IDbContextCreationLifecycle _modelCreationLifecycle;

    public TemplateAppDbContext(DbContextOptions<TemplateAppDbContext> options, IDbContextCreationLifecycle modelCreationLifecycle)
        : base(options) =>
        _modelCreationLifecycle = modelCreationLifecycle;

    protected TemplateAppDbContext(IDbContextCreationLifecycle modelCreationLifecycle) =>
        _modelCreationLifecycle = modelCreationLifecycle;

    public DbSet<Article> Articles { get; set; } = null!;

    public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;

    public DbSet<ArticleTag> ArticleTags { get; set; } = null!;

    public DbSet<ArticleProperty> ArticleProperties { get; set; } = null!;

    public DbSet<Author> Authors { get; set; } = null!;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemplateAppDbContext).Assembly);
        _modelCreationLifecycle.OnModelCreating(modelBuilder, Database);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        _modelCreationLifecycle.OnConfiguring(optionsBuilder);
    }

    private void SetAuditTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IHasAuditTimeProperties>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedTime = now;
                    entry.Entity.ModifiedTime = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedTime = now;
                    break;
            }
        }
    }
}
