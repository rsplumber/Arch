using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Extension
{
    public  class Parameter
    {
        //key is url(path of Request)
        public static IDictionary<string, LimitCondition> RateLimitParameters { get; set; }
        = new Dictionary<string, LimitCondition>();

        public Parameter AddRoute(string url , LimitCondition condition)
        {
            RateLimitParameters.Add(url , condition);
            return this;
        }
    }


    public class LimitCondition
    {
        public string IdentifierInRequestBody { get; set; }
        public TimeSpan WindowsSize { get; set; }
        public int MaxAllowdRequestInWindow { get; set; }
        //public int LimitePeriodInMinute { get; set; }
        public string BriefURL { get; set; }
    }







}
