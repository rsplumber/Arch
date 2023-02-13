using Core.EndpointDefinitions;
using Core.Metas;
using Core.ServiceConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Sql;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
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

            builder.Property(serviceConfig => serviceConfig.Id)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("id");

            builder.Property(serviceConfig => serviceConfig.Name)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("name");

            builder.Property(serviceConfig => serviceConfig.BaseUrl)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("base_url");

            builder.Property(serviceConfig => serviceConfig.Primary)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("primary");

            builder.Property(serviceConfig => serviceConfig.CreatedAtUtc)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("created_at_utc");

            builder.HasMany(serviceConfig => serviceConfig.EndpointDefinitions)
                .WithOne(definition => definition.ServiceConfig)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(config => config.Id)
                .HasForeignKey("service_config_id");

            builder.HasMany(serviceConfig => serviceConfig.Meta)
                .WithOne(meta => meta.ServiceConfig)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(config => config.Id)
                .HasForeignKey("service_config_id");
        }
    }

    private class BinderEntityTypeConfiguration : IEntityTypeConfiguration<EndpointDefinition>
    {
        public void Configure(EntityTypeBuilder<EndpointDefinition> builder)
        {
            builder.ToTable("endpoint_definitions")
                .HasKey(definition => definition.Id);

            builder.Property(definition => definition.Id)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("id");

            builder.Property(definition => definition.Pattern)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("pattern");

            builder.Property(definition => definition.Endpoint)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("endpoint");

            builder.Property(definition => definition.Method)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("method");

            builder.HasMany(definition => definition.Meta)
                .WithOne(meta => meta.EndpointDefinition)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(definition => definition.Id)
                .HasForeignKey("endpoint_definition_id");
        }
    }

    private class MetaEntityTypeConfiguration : IEntityTypeConfiguration<Meta>
    {
        public void Configure(EntityTypeBuilder<Meta> builder)
        {
            builder.ToTable("meta")
                .HasKey(meta => meta.Id);

            builder.Property(meta => meta.Id)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("id");

            builder.Property(meta => meta.Key)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("key");

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