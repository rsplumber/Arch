using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimit.Cage.Extension
{
    public static class RemindedTime
    {
        public static string Calculate(DateTime startDate, TimeSpan addedTime)
        {
            var endDate = startDate + addedTime;
            var totalSeconds = (int)(endDate - DateTime.UtcNow).TotalSeconds;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes}:{seconds}";
        }
    }
}
