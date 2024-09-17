using Encryption.Abstractions;
using Encryption.Tes.Security.Domain;
using Encryption.Tes.Security.Endpoints.Key;
using Encryption.Tes.Security.Infrastructure;
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
        var connectionString = configuration.GetConnectionString("TesEncryption");
        options.Services.AddDbContextPool<EncryptionDbContext>(op => { op.UseNpgsql(connectionString); }, poolSize: 200);
    }
}