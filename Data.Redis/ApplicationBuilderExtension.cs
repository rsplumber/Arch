using Data.Redis.Binders;
using Data.Redis.ServiceConfigs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM;

namespace Data.Redis;

internal static class ApplicationBuilderExtension
{
    public static void UseData(this IApplicationBuilder app)
    {
        try
        {
            var dbProvider = app.ApplicationServices.GetRequiredService<RedisConnectionProvider>();
            new List<Type>
            {
                typeof(BinderDataModel),
                typeof(ServiceConfigDataModel),
            }.ForEach(type =>
            {
                if (dbProvider.Connection.GetIndexInfo(type) is null)
                {
                    dbProvider.Connection.CreateIndex(type);
                }
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }
}