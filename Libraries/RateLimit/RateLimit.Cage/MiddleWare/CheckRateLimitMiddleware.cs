using Arch.Core.Extensions.Http;
using Arch.Core.Pipeline.Models;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using RateLimit.Cage.Extension;
using RateLimit.Cage.Services;
using Arch.Core.Pipeline;

namespace RateLimit.Cage.MiddleWare
{
    public class CheckRateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        public CheckRateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.RequestState().RequestInfo.Headers.TryGetValue("version", out string? value);
            if (string.IsNullOrEmpty(value) || int.Parse(value) < RateLimitDefault.Version)
            {
                await _next(context);
                return;
            }


            var _cacheService = context.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            var _service = context.RequestServices.GetService(typeof(RateLimitServices)) as RateLimitServices;
            var ip = context.Connection.RemoteIpAddress;
            var endpointDefinition = context.RequestState().EndpointDefinition;
            var limitConditions = _service.FillOptions(endpointDefinition);
            var requestState = context.RequestState();
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
                    await context.Response.SendAsync(new Response
                    {
                        RequestId = requestState.RequestInfo.RequestId,
                        RequestDateUtc = requestState.RequestInfo.RequestDateUtc,
                        Data = "درخواست شما نامعتبر است"
                    }, 400).ConfigureAwait(false);
                    return;
                }

                if (context.Request.HasBody())
                {
                    Identifier = await _service.ReadRequestBody(context, limitConditions);
                }

                if (string.IsNullOrEmpty(Identifier))
                {

                    await context.Response.SendAsync(new Response
                    {
                        RequestId = requestState.RequestInfo.RequestId,
                        RequestDateUtc = requestState.RequestInfo.RequestDateUtc,
                        Data = "درخواست شما نامعتبر است"
                    }, 400).ConfigureAwait(false);
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
                await context.Response.SendAsync(new Response
                {
                    RequestId = requestState.RequestInfo.RequestId,
                    RequestDateUtc = requestState.RequestInfo.RequestDateUtc,
                    Data = new
                    {
                        message = "Blocked",
                        clientMessage = message
                    }
                }, 429).ConfigureAwait(false);
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
                return rateLimitState.Count > limitConditions.MaxAllowedRequestInWindow;
            }

            TimeSpan calculateBlockTimeCondition()
            {
                return (globalBlocked) ? RateLimitDefault.WindowsSize : limitConditions.WindowsSize;
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

}