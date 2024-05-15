using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline.Models;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RateLimit.Cage.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Services
{
    public class RateLimitServices
    {
        private readonly IMemoryCache _cacheService;
        public RateLimitServices(IMemoryCache cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<string?> ReadRequestBody(HttpContext context, LimitCondition specialCondition)
        {
            context.Request.EnableBuffering();
            var reqBody = await context.Request.ReadFromJsonAsync<dynamic>();
            context.Request.Body.Position = 0;
            var reqBodyAsJson = System.Text.Json.JsonSerializer.Serialize(reqBody);
            var data = (JObject)JsonConvert.DeserializeObject(reqBodyAsJson);
            var identifier = data.Value<JToken>()[specialCondition.IdentifierInRequestBody];
            
            return identifier?.ToString();



        }

      

        public async Task<MemoryCacheEntryOptions> CacheOptions(int size, CacheItemPriority Priority, TimeSpan SlidingExpiration)
        {
            return new MemoryCacheEntryOptions()
            {
                Size = size,
                Priority = Priority,
                SlidingExpiration = SlidingExpiration,

            };


        }

        public string ConcatKey(string Identifier, string briefURL)
        {
            return $"{Identifier}_{briefURL}";
        }

        public LimitCondition FillOptions(EndpointDefinition? endpointDefinition)
        {
            var isSpecial = endpointDefinition.Meta.Any(x => x.Key == "rate_limit");

            return new LimitCondition
            {
                IdentifierInRequestBody = endpointDefinition.Meta.SingleOrDefault(x => x.Key == "identifier_request_body").Value ?? "userid",
                MaxAllowdRequestInWindow = (isSpecial) ? int.Parse(endpointDefinition.Meta["max_allowd_request_in_window"]) : RateLimitDefault.MaxAllowdRequestInWindow,
                WindowsSize = (isSpecial) ? TimeSpan.Parse(endpointDefinition.Meta["window_size"]) : RateLimitDefault.WindowsSize
            };
        }


    }
}
