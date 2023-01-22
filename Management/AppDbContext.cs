using Core.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Management;

public class ManagementDbContext : DbContext
{
    public ManagementDbContext(DbContextOptions<ManagementDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceConfig> ServiceConfigs { get; set; }

    public DbSet<EndpointDefinition> EndpointDefinitions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ServiceConfigEntityTypeConfiguration());
        builder.ApplyConfiguration(new BinderEntityTypeConfiguration());
        builder.ApplyConfiguration(new MetaEntityTypeConfiguration());
        base.OnModelCreating(builder);
    }

    private class ServiceConfigEntityTypeConfiguration : IEntityTypeConfiguration<ServiceConfig>
    {
        public void Configure(EntityTypeBuilder<ServiceConfig> builder)
        {
            builder.ToTable("service_configs")
                .HasKey(serviceConfig => serviceConfig.Id);

            builder.Property(serviceConfig => serviceConfig.Name)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("name");

            builder.Navigation(b => b.EndpointDefinitions)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.Navigation(b => b.Meta)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.Property(serviceConfig => serviceConfig.BaseUrl)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("base_url");

            builder.HasMany(serviceConfig => serviceConfig.EndpointDefinitions);

            builder.HasMany(serviceConfig => serviceConfig.Meta);
        }
    }

    private class BinderEntityTypeConfiguration : IEntityTypeConfiguration<EndpointDefinition>
    {
        public void Configure(EntityTypeBuilder<EndpointDefinition> builder)
        {
            builder.ToTable("endpoint_definitions")
                .HasKey(definition => definition.Pattern);

            builder.Navigation(definition => definition.Meta)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.HasMany(definition => definition.Meta);

            builder.Property(definition => definition.Endpoint)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("endpoint");
        }
    }

    private class MetaEntityTypeConfiguration : IEntityTypeConfiguration<Meta>
    {
        public void Configure(EntityTypeBuilder<Meta> builder)
        {
            builder.ToTable("meta")
                .HasKey(meta => meta.Id);

            builder.Property(meta => meta.Value)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("value");
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}