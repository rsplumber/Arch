using Microsoft.Extensions.DependencyInjection;

namespace Arch.Configurations;

/// <summary>
/// Describes a single Arch library/module that was wired into the gateway programmatically
/// at startup (e.g. the chosen Encryption, Authorization or Endpoint Graph provider).
/// </summary>
public sealed record ArchLibraryDescriptor
{
    /// <summary>Category the library plugs into (e.g. "Authorization", "Encryption").</summary>
    public required string Category { get; init; }

    /// <summary>Concrete provider name (e.g. "Archrypt", "Kundera", "In-Memory").</summary>
    public required string Provider { get; init; }

    /// <summary>Optional human friendly description shown in the admin panel.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Collects the libraries registered while <c>AddArch</c> runs so they can be surfaced
/// (read-only) in tooling such as the Admin panel.
/// </summary>
public interface IArchLibraryRegistry
{
    IReadOnlyCollection<ArchLibraryDescriptor> Libraries { get; }

    void Register(ArchLibraryDescriptor descriptor);
}

internal sealed class ArchLibraryRegistry : IArchLibraryRegistry
{
    private readonly List<ArchLibraryDescriptor> _libraries = [];

    public IReadOnlyCollection<ArchLibraryDescriptor> Libraries => _libraries.AsReadOnly();

    public void Register(ArchLibraryDescriptor descriptor)
    {
        // Last registration for a category/provider pair wins so re-registration is idempotent.
        _libraries.RemoveAll(library => library.Category == descriptor.Category && library.Provider == descriptor.Provider);
        _libraries.Add(descriptor);
    }
}

public static class ArchLibraryRegistryExtensions
{
    /// <summary>
    /// Records that a concrete Arch library has been registered. The registry singleton is
    /// created lazily on first call so any provider extension can self-register regardless of order.
    /// </summary>
    public static IServiceCollection RegisterArchLibrary(this IServiceCollection services, ArchLibraryDescriptor descriptor)
    {
        GetOrAddRegistry(services).Register(descriptor);
        return services;
    }

    public static IServiceCollection RegisterArchLibrary(this IServiceCollection services, string category, string provider, string? description = null)
        => services.RegisterArchLibrary(new ArchLibraryDescriptor
        {
            Category = category,
            Provider = provider,
            Description = description
        });

    private static ArchLibraryRegistry GetOrAddRegistry(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(service => service.ServiceType == typeof(IArchLibraryRegistry));
        if (descriptor?.ImplementationInstance is ArchLibraryRegistry existing)
        {
            return existing;
        }

        var registry = new ArchLibraryRegistry();
        services.AddSingleton<IArchLibraryRegistry>(registry);
        return registry;
    }
}
