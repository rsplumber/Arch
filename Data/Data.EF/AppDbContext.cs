using Arch.Core.EndpointDefinitions;
using Arch.Core.Metas;
using Arch.Core.ServiceConfigs;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arch.Data.EF;

public class AppDbContext : DbContext
{
    private readonly ICapPublisher _eventBus;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICapPublisher eventBus) : base(options)
    {
        _eventBus = eventBus;
    }

    public DbSet<ServiceConfig> ServiceConfigs { get; set; }

    public DbSet<EndpointDefinition> EndpointDefinitions { get; set; }

    public DbSet<Meta> Metas { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ServiceConfigEntityTypeConfiguration());
        builder.ApplyConfiguration(new EndpointDefinitionEntityTypeConfiguration());
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

            builder.HasIndex(config => config.Name);

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

    private class EndpointDefinitionEntityTypeConfiguration : IEntityTypeConfiguration<EndpointDefinition>
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

            builder.HasIndex(definition => definition.Pattern);

            builder.Property(definition => definition.Endpoint)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("endpoint");

            builder.HasIndex(definition => definition.Endpoint);

            builder.Property(definition => definition.MapTo)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("map_to");

            builder.HasIndex(definition => definition.MapTo);

            builder.Property(definition => definition.Method)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasConversion(method => method.ToString(), s => new HttpMethod(s))
                .HasColumnName("method");

            builder.HasIndex(definition => definition.Pattern);

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

            builder.HasIndex(definition => definition.Key);

            builder.Property(meta => meta.Value)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("value");
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _eventBus.DispatchDomainEventsAsync(this);
        return await base.SaveChangesAsync(cancellationToken);
    }
}