using Microsoft.EntityFrameworkCore;
using SecureFileUploader.Data.Configurations;
using SecureFileUploader.Data.Entities;

namespace SecureFileUploader.Data;

/// <summary>
/// Represents the session with the database and allows for querying and saving instances of entities.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the DbSet of Users.
    /// </summary>
    /// <value>A set for access to users within the context.</value>
    public DbSet<User> Users { get; set; }
        
    /// <summary>
    /// Gets or sets the DbSet of Files.
    /// </summary>
    /// <value>A set for access to files within the context.</value>
    public DbSet<Entities.File> Files { get; set; }

    /// <summary>
    /// Configures the schema needed for the context before the model is locked down and used to initialize the context.
    /// This method is called for each instance of the context that is created.
    /// </summary>
    /// <param name="modelBuilder">Defines the shape of your entities, the relationships between them, and how they map to the database.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FileConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc cref="DbContext.SaveChanges()"/>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        var timestampEntries = ChangeTracker.Entries()
            .Where(e => e is { Entity: TimestampEntity, State: EntityState.Added or EntityState.Modified });

        foreach (var entry in timestampEntries)
        {
            var entity = (TimestampEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedOn = utcNow;
            }

            entity.UpdatedOn = utcNow;
        }
    }
}