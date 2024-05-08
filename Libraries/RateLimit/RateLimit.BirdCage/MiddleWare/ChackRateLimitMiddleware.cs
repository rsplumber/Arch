using Arch.Core.Extensions.Http;
using DotNetCore.CAP.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RateLimit.Extension;
using RateLimit.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.MiddleWare
{
    public class ChackRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cacheService;

        public ChackRateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var _cacheService = context.RequestServices.GetService(typeof(IMemoryCache)) as IMemoryCache;
            LimitCondition condition;

            Parameter.RateLimitParameters.TryGetValue(context.Request.Path, out LimitCondition _condition);
            condition = _condition;

            string? Identifier;
            if (condition is not null)
            {
                var reqBody = await context.Request.ReadFromJsonAsync<dynamic>();
                var reqBodyAsJson = System.Text.Json.JsonSerializer.Serialize(reqBody);
                var data = (JObject)JsonConvert.DeserializeObject(reqBodyAsJson);
                Identifier = (string)data["body"].Value<JToken>()[condition.IdentifierInRequestBody];
                if (string.IsNullOrEmpty(Identifier))
                {
                    //string message = $"به دلیل درخواست های مکرر حساب شما تا {RemindedTime.Calculate(rateLimitState.LastAccess, condition.WindowsSize)} دیگر مسدود شده است";
                    //var result = new ObjectResult(new { message = message })
                    //{
                    //    StatusCode = StatusCodes.Status429TooManyRequests,
                    //};
                    //await context.Response.WriteAsJsonAsync(result);
                    await Task.CompletedTask;
                    return;

                }
            }
            else
            {
                Identifier = context.RequestState().RequestInfo;
                condition = new LimitCondition()
                {
                    MaxAllowdRequestInWindow = 5,
                    WindowsSize = TimeSpan.FromMinutes(5),

                };
            }
            // if in body wasnt  any keys i wanted .. what should to do?
            //

            var key = ConcatKey((string)Identifier, condition.BriefURL);

            var now = DateTime.UtcNow;
            RateLimitState rateLimitState;
            rateLimitState = (RateLimitState)_cacheService.Get(key);
            if (rateLimitState is null)
            {
                rateLimitState = new RateLimitState(now);
                _cacheService.Set(key, rateLimitState);
            }

            if (now - rateLimitState.LastAccess > condition.WindowsSize)
            {
                rateLimitState.Count = 0;
                rateLimitState.LastAccess = now;
            }
            rateLimitState.Count++;
            var isLimited = rateLimitState.Count > condition.MaxAllowdRequestInWindow;

            if (isLimited)
            {
                string message = $"به دلیل درخواست های مکرر حساب شما تا {RemindedTime.Calculate(rateLimitState.LastAccess, condition.WindowsSize)} دیگر مسدود شده است";
                var result = new ObjectResult(new { message = message })
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                };
                await context.Response.WriteAsJsonAsync(result);
                await Task.CompletedTask;
                return;
            }

            await _next(context);


        }



        private string ConcatKey(string Identifier, string briefURL)
        {
            return $"{Identifier}_{briefURL}";
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
