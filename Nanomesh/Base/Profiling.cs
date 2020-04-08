using System;
using System.Diagnostics;

namespace Nanolabo
{
    public static class Profiling
    {
        public static TimeSpan Time(Action toTime)
        {
            var timer = Stopwatch.StartNew();
            toTime();
            timer.Stop();
            return timer.Elapsed;
        }
    }
}
