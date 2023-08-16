﻿using Data.Abstractions;

namespace Caching.Abstractions;

public static class ServiceCollectionExtension
{
    public static void AddCaching(this DataOptions dataOptions, Action<CachingOptions>? options = null)
    {
        options?.Invoke(new CachingOptions
        {
            Services = dataOptions.Services
        });
    }
}