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

    public DbSet<Binder> Binders { get; set; }
    
    public DbSet<Meta> Metas { get; set; }


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
            builder.HasMany(serviceConfig => serviceConfig.Binders)
                .WithOne();

            builder.Navigation(b => b.Binders)
                .UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.ToTable("service_configs")
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

    private class BinderEntityTypeConfiguration : IEntityTypeConfiguration<Binder>
    {
        public void Configure(EntityTypeBuilder<Binder> builder)
        {
            builder.ToTable("binders")
                .HasKey(binder => binder.Id);

            builder.HasMany(binder => binder.Metas)
                .WithOne();

            builder.Navigation(binder => binder.Metas)
                .UsePropertyAccessMode(PropertyAccessMode.Property);


            builder.Property(binder => binder.ApiUrl)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("api_url");
        }
    }

    private class MetaEntityTypeConfiguration : IEntityTypeConfiguration<Meta>
    {
        public void Configure(EntityTypeBuilder<Meta> builder)
        {
            builder.ToTable("metas")
                .HasKey(meta => meta.Id);

            builder.Property(meta => meta.Value)
                .UsePropertyAccessMode(PropertyAccessMode.Property)
                .HasColumnName("value");
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}