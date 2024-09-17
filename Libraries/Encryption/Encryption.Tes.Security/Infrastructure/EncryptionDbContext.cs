using Encryption.Tes.Security.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Encryption.Tes.Security.Infrastructure;

public class EncryptionDbContext : DbContext
{
    private const string Schema = "encryption";

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
            builder.ToTable("version_key", Schema)
                .HasKey(model => model.Version);

            builder.Property(model => model.Version)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("version");

            builder.Property(model => model.Key)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("key")
                .IsRequired();

            builder.Property(model => model.CreateDateUtc)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("create_date_utc")
                .IsRequired();
        }
    }
}