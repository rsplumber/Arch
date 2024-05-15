using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using RateLimit.Cage.Extension;
using RateLimit.Cage.Services;

namespace RateLimit.Cage.MiddleWare
{
    public class ChackRateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        public ChackRateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.RequestState().RequestInfo.Headers.TryGetValue("version", out string? value);
            if (string.IsNullOrEmpty(value) || int.Parse(value) < 120)
            {
                await _next(context);
                return;
            }


            var _cacheService = context.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            var _service = context.RequestServices.GetService(typeof(RateLimitServices)) as RateLimitServices;
            var ip = context.Connection.RemoteIpAddress;
            var endpointDefinition = context.RequestState().EndpointDefinition;
            var limitConditions = _service.FillOptions(endpointDefinition);
            string key;
            bool isLimited = false;
            bool isDefaultConditions = false;
            bool globalBlocked = false;

            if (!isSpecial())
                isDefaultConditions = true;

            string? Identifier = string.Empty;
            if (!isDefaultConditions)
            {
                if (!requestHasBodyAndIsSpecial())
                {
                    await context.Response.SendAsync(new ResponseInfo()
                    {
                        Code = 400,
                        Headers = new Dictionary<string, string>(),
                        ResponseTimeMilliseconds = 1,
                        Value = "درخواست شما نامعتبر است",
                    }, 400);
                    return;
                }

                if (context.Request.HasBody())
                {
                    Identifier = await _service.ReadRequestBody(context, limitConditions);
                }

                if (string.IsNullOrEmpty(Identifier))
                {
                    await context.Response.SendAsync(new ResponseInfo()
                    {
                        Code = 400,
                        Headers = new Dictionary<string, string>(),
                        ResponseTimeMilliseconds = 1,
                        Value = "درخواست شما نامعتبر است",
                    }, 400);
                    return;
                }
            }

            key = generateKeyForGlobal();
            var rateLimitState = _cacheService.Get(key) as RateLimitState ?? null;

            if (rateLimitState != null)
            {
                if (isRequestInWindow(GlobaConditions.Values().WindowsSize))
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

                if (!isRequestInWindow(limitConditions.WindowsSize))
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
                await context.Response.SendAsync(new ResponseInfo()
                {
                    Code = 429,
                    Headers = new Dictionary<string, string>(),
                    ResponseTimeMilliseconds = 1,
                    Value = message,
                }, 429);
                return;
            }

            await _next(context);


            bool isSpecial() => context.RequestState().EndpointDefinition.Meta.Any(x => x.Key == "rate_limit");

            bool requestHasBodyAndIsSpecial()
            {
                return context.Request.HasBody() && context.RequestState().EndpointDefinition.Meta.Any(x => x.Key == "identifier_request_body");
            }

            bool isRequestInWindow(TimeSpan windowSize)
            {
                var now = DateTime.UtcNow;
                var divided = now - rateLimitState.LastAccess;
                var res = divided < windowSize;
                return res;
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
                return _service.ConcatKey(ip.ToString(), GlobaConditions.Values().BriefURL);
            }

            string generateKeyForDefaultOrSpecial()
            {
                string secondPart = (isDefaultConditions) ? GlobaConditions.Values().BriefURL : endpointDefinition.Endpoint;

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