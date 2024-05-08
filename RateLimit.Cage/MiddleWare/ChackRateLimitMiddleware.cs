using Arch.Core.Extensions.Http;
using Arch.Core.ServiceConfigs.EndpointDefinitions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RateLimit.Cage.Extension;
using RateLimit.Cage.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.MiddleWare
{
    public class ChackRateLimitMiddleware : IMiddleware
    {
        private readonly RequestDelegate _next;
        private RateLimitState rateLimitState;
        string key;
        bool isLimited = false;
        bool isDefaultConditions = false;
        bool globalBlocked = false;

        public ChackRateLimitMiddleware(RequestDelegate next)
        {
           
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var _cacheService = context.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            var _service = context.RequestServices.GetService(typeof(RateLimitServices)) as RateLimitServices;
            var ip = context.Connection.RemoteIpAddress;
            var endpointDefinition = context.RequestState().EndpointDefinition;
            var limitConditions = _service.FillOptions(endpointDefinition);


            if (!isSpecial())
                isDefaultConditions = true;

            string? Identifier = string.Empty;
            if (!isDefaultConditions)
            {
                if (!requestHasBodyAndIsSpecial())
                {
                    await _service.FillErrorResponse(context, "درخواست شما نامعتبر است", 400);
                    return;
                }
                if (context.Request.HasBody())
                {
                    Identifier = await _service.ReadRequestBody(context, limitConditions);

                }
                if (string.IsNullOrEmpty(Identifier))
                {
                    await _service.FillErrorResponse(context, "درخواست شما نامعتبر است", 400);
                    return;
                }
            }

            key = generateKeyForGlobal();
            rateLimitState = _cacheService.Get(key) as RateLimitState ?? null;

            if (rateLimitState != null)
            {
                if (isRequestInWindow())
                {
                    isLimited = shouldRequestBlock();
                    if (isLimited)
                        globalBlocked = true;

                }

            }

            if (!isLimited)
            {
                key = generateKeyForDefaultOrSpecial();
                rateLimitState = (RateLimitState)_cacheService.Get(key);
                if (rateLimitState is null)
                {
                    await setFirstRateLimitRecord();
                }

                if (!isRequestInWindow())
                {
                    resetLimitState();
                }
                rateLimitState.Count++;
                isLimited = shouldRequestBlock();

            }

            if (isLimited)
            {
                var blockTime = calculateBlockTimeCondition();
                string message = $"به دلیل درخواست های مکرر حساب شما تا {RemindedTime.Calculate(rateLimitState.LastAccess, blockTime)} دیگر مسدود شده است";
                await _service.FillErrorResponse(context, message, 429);

                return;
            }

            await _next(context);






            bool isSpecial()
            {
                return context.RequestState().EndpointDefinition.Meta.Any(x => x.Key == "rate_limit");
            }

            bool requestHasBodyAndIsSpecial()
            {
                return context.Request.HasBody() && context.RequestState().EndpointDefinition.Meta.Any(x => x.Key == "identifier_request_body");
            }

            bool isRequestInWindow()
            {
                return DateTime.UtcNow - rateLimitState.LastAccess < limitConditions.WindowsSize;
            }

            bool shouldRequestBlock()
            {
                return rateLimitState.Count > limitConditions.MaxAllowdRequestInWindow;
            }

            TimeSpan calculateBlockTimeCondition()
            {
                return (globalBlocked) ? TimeSpan.FromMinutes(1) : limitConditions.WindowsSize;
            }

            string generateKeyForGlobal()
            {
                return _service.ConcatKey(ip.ToString(), "global");
            }

            string generateKeyForDefaultOrSpecial()
            {
                string secondPart = (isDefaultConditions) ? "global" : endpointDefinition.Endpoint;

                var firstPart = (isDefaultConditions) ? ip.ToString() : Identifier;
                return _service.ConcatKey(firstPart, secondPart);

            }

            void resetLimitState()
            {
                rateLimitState.Count = 0;
                rateLimitState.LastAccess = DateTime.UtcNow;

            }

            async Task setFirstRateLimitRecord()
            {
                rateLimitState = new RateLimitState(DateTime.UtcNow);
                var cacheOptions = await _service.CacheOptions(500, CacheItemPriority.Normal, TimeSpan.FromMinutes(10));
                _cacheService.Set(key, rateLimitState, cacheOptions);

            }
        }
    }
    public class RateLimitState
    {
        public int Count { get; set; }
        public DateTime LastAccess { get; set; }

        public RateLimitState(DateTime lastAccess)
        {
            Count = 0;
            LastAccess = lastAccess;
        }
    }

}
