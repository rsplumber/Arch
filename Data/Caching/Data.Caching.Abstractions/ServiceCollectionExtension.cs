﻿using Arch.Data.Abstractions;

namespace Arch.Data.Caching.Abstractions;

public static class DataOptionsExtension
{
    public static void AddCaching(this DataOptions dataOptions, Action<CachingOptions>? options = null)
    {
        options?.Invoke(new CachingOptions
        {
            Services = dataOptions.Services
        });
    }
}