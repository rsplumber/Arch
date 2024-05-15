using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Extension
{
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
