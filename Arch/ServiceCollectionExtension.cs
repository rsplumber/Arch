using Arch.Endpoints.Shahkar;

namespace Arch;

public static class ServiceCollectionExtension
{
    public static void AddArch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ShahkarOptions>(configuration.GetSection("Shahkar"));
    }
}