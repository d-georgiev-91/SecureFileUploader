using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SecureFileUploader.Data.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<Entities.File>
{
    public void Configure(EntityTypeBuilder<Entities.File> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(100);
        builder.Property(f => f.ContentType).IsRequired().HasMaxLength(20);
        builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(256);

        builder.HasOne(f => f.User)
               .WithMany(f => f.Files)
               .HasForeignKey(f => f.UserId);

        builder.HasIndex(f => new { f.FileName, f.UserId }).IsUnique();
        builder.HasIndex(f => f.StoragePath).IsUnique();
    }
}