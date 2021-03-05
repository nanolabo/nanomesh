using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh
{
    public static class Profiling
    {
        private static readonly Dictionary<string, Stopwatch> stopwatches = new Dictionary<string, Stopwatch>();

        public static void Start(string key)
        {
            if (!stopwatches.ContainsKey(key))
            {
                stopwatches.Add(key, Stopwatch.StartNew());
            }
            else
            {
                stopwatches[key] = Stopwatch.StartNew();
            }
        }

        public static string End(string key)
        {
            TimeSpan time = EndTimer(key);
            return $"{key} done in {time.ToString("mm':'ss':'fff")}";
        }

        private static TimeSpan EndTimer(string key)
        {
            if (!stopwatches.ContainsKey(key))
            {
                return TimeSpan.MinValue;
            }

            Stopwatch sw = stopwatches[key];
            sw.Stop();
            stopwatches.Remove(key);
            return sw.Elapsed;
        }

        public static TimeSpan Time(Action toTime)
        {
            Stopwatch timer = Stopwatch.StartNew();
            toTime();
            timer.Stop();
            return timer.Elapsed;
        }
    }
}
