using Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Sql;

public class ArchDbContext : DbContext
{
    public ArchDbContext(DbContextOptions<ArchDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceConfig> ServiceConfigs { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ServiceConfigEntityTypeConfiguration());
        base.OnModelCreating(builder);
    }

    private class ServiceConfigEntityTypeConfiguration : IEntityTypeConfiguration<ServiceConfig>
    {
        public void Configure(EntityTypeBuilder<ServiceConfig> builder)
        {
            builder.ToTable("serviceConfigs")
                .HasKey(serviceConfig => serviceConfig.Id);

            builder.Property(serviceConfig => serviceConfig.Name)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("name");

            builder.Property(serviceConfig => serviceConfig.Secret)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("secret");

            builder.Property(serviceConfig => serviceConfig.BaseUrl)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("base_url");
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}