using Encryption.Abstractions;
using Encryption.Tes.Security.Domain;
using Encryption.Tes.Security.Endpoints.Key;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Encryption.Tes.Security;

public static class EncryptionExecutionOptionsExtension
{
    public static void UseTesSecurityEncryption(this EncryptionOptions options, IConfiguration configuration)
    {
        options.Services.AddSingleton<TesSecurityRequestEncryptionMiddleware>();
        options.Services.AddSingleton<TesSecurityResponseEncryptionMiddleware>();
        options.Services.AddScoped<IKeyManagement, KeyManagement>();
        options.Services.AddScoped<IVersionKeyRepository, VersionKeyRepository>();

        options.Services.AddDistributedMemoryCache();

        var connectionString = "User ID=tesdbadmin;Password=Tes@DBPass@;Host=192.168.67.17;Port=5432;Database=arch;Pooling=true;";
        options.Services.AddDbContextPool<EncryptionDbContext>(options => { options.UseNpgsql(connectionString); }, poolSize: 500);
    }
}