using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Encryption.Tes.Security.Domain;

public class EncryptionDbContext : DbContext
{
    public EncryptionDbContext(DbContextOptions<EncryptionDbContext> options) : base(options)
    {
    }

    public DbSet<VersionKey> VersionKey { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new VersionKeyEntityTypeConfiguration());
        base.OnModelCreating(builder);
    }

    private class VersionKeyEntityTypeConfiguration : IEntityTypeConfiguration<VersionKey>
    {
        public void Configure(EntityTypeBuilder<VersionKey> builder)
        {
            builder.ToTable("version_key")
                .HasKey(model => model.Id);

            builder.Property(model => model.Id)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("id");

            builder.Property(model => model.Key)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("key")
                .IsRequired(true);

            builder.Property(model => model.Version)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("version")
                .IsRequired(true);
            builder.HasIndex(model => model.Version).IsUnique();

            builder.Property(model => model.CreateDateUtc)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("create_date_utc")
                .IsRequired(true);
        }
    }
}