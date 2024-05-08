using Microsoft.Extensions.Caching.Memory;
using RateLimit.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Services
{
    public class RateLimitServices
    {
        private readonly Parameter _params;
        private readonly IMemoryCache _cacheService;
        public RateLimitServices(Parameter para, IMemoryCache cacheService)
        {
            _params = para;
            _cacheService = cacheService;
        }

        public async Task ReadRequestBody(dynamic reqBody, string url)
        {
            LimitCondition condition;

            Parameter.RateLimitParameters.TryGetValue(url, out LimitCondition _condition);
            condition = _condition ?? new LimitCondition()
            {
                IdentifierInRequestBody = "userId",
                MaxAllowdRequestInWindow = 5,
                WindowsSize = TimeSpan.FromMinutes(5),

            };

            // should find identifier from body
            var Identifier = reqBody[condition.IdentifierInRequestBody];
            //

            var key =  ConcatKey((string)Identifier, condition.BriefURL);


        }

        public RateLimitResponse CheckRateLimit(string key , LimitCondition condition )
        {
            var now = DateTime.UtcNow;
            RateLimitState rateLimitState;
            rateLimitState =(RateLimitState) _cacheService.Get(key) ;
            if(rateLimitState is null)
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

            return new RateLimitResponse()
            {
                Status = isLimited,
                LimitedTime = (isLimited) ? RemindedTime.Calculate(rateLimitState.LastAccess, condition.WindowsSize) : null
            };


        }

        private string ConcatKey(string Identifier , string briefURL)
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

    public class RateLimitResponse
    {
        public bool Status { get; set; }
        public string? LimitedTime { get; set; } = null;
    }


}
