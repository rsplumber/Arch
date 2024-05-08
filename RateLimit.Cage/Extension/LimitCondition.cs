using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Extension
{
    public class LimitCondition
    {
        public string IdentifierInRequestBody { get; set; } 
        public TimeSpan WindowsSize { get; set; } 
        public int MaxAllowdRequestInWindow { get; set; }
        public string BriefURL { get; set; } 
    }
}
