using System.Text.Json;
using Arch.Core.ServiceConfigs;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arch.Data.EF;

public class AppDbContext : DbContext
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        IgnoreReadOnlyFields = true
    };

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ServiceConfig> ServiceConfigs { get; set; }

    public DbSet<EndpointDefinition> EndpointDefinitions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new ServiceConfigEntityTypeConfiguration());
        builder.ApplyConfiguration(new EndpointDefinitionEntityTypeConfiguration());
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

            builder.Property(serviceConfig => serviceConfig.BaseUrls)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasConversion(password => JsonSerializer.Serialize(password, JsonSerializerOptions),
                    s => JsonSerializer.Deserialize<List<string>>(s, JsonSerializerOptions)!)
                .HasColumnName("base_urls");

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


            builder.Property(serviceConfig => serviceConfig.Meta)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasConversion(password => JsonSerializer.Serialize(password, JsonSerializerOptions),
                    s => JsonSerializer.Deserialize<Dictionary<string, string>>(s, JsonSerializerOptions)!)
                .HasColumnName("meta")
                .IsRequired(false);
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

            builder.Property(serviceConfig => serviceConfig.Meta)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasConversion(password => JsonSerializer.Serialize(password, JsonSerializerOptions),
                    s => JsonSerializer.Deserialize<Dictionary<string, string>>(s, JsonSerializerOptions)!)
                .HasColumnName("meta")
                .IsRequired(false);
        }
    }
}