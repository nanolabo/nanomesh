using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanolabo
{
    public static class Profiling
    {
        private static Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();

        public static void Start(string key)
        {
            if (!stopwatches.ContainsKey(key))
                stopwatches.Add(key, Stopwatch.StartNew());
            else
            {
                stopwatches[key] = Stopwatch.StartNew();
            }
        }

        public static void End(string key)
        {
            TimeSpan time = EndTimer(key);
            Console.WriteLine($"{key} done in {time.ToString("mm':'ss':'fff")}");
        }

        private static TimeSpan EndTimer(string key)
        {
            if (!stopwatches.ContainsKey(key))
                return TimeSpan.MinValue;
            Stopwatch sw = stopwatches[key];
            sw.Stop();
            stopwatches.Remove(key);
            return sw.Elapsed;
        }

        public static TimeSpan Time(Action toTime)
        {
            var timer = Stopwatch.StartNew();
            toTime();
            timer.Stop();
            return timer.Elapsed;
        }
    }
}
