using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Extension
{
    public static class RateLimitDefault
    {
        public static int MaxAllowdRequestInWindow { get; set; }
        public static TimeSpan WindowsSize { get; set; }
        public static int Version { get; set; }
    }
}
