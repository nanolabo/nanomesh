using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Nanomesh.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using static Nanomesh.DecimateModifier;

namespace Nanomesh.Benchmarks
{
    [MemoryDiagnoser]
    //[MonoJob("Mono x64", @"C:\Program Files\Mono\bin\mono.exe")]
    //[SimpleJob(RuntimeMoniker.Mono, launchCount: 1, warmupCount: 2, targetCount: 5)]
    //[SimpleJob(RuntimeMoniker.CoreRt31, launchCount: 1, warmupCount: 2, targetCount: 5)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp50, launchCount: 1, warmupCount: 2, targetCount: 5)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31, launchCount: 1, warmupCount: 2, targetCount: 5)]
    public class SortingBenchmark
    {
        [Params(true, false)]
        public bool Precise { get; set; }

        [Params(10, 100, 500, 1000, 2000, 5000, 10000)]
        public int MinsCount { get; set; }

        [Params(10000, 50000, 100000)]
        public int PairCount { get; set; }

        private HashSet<EdgeCollapse> _pairs;

        [IterationSetup]
        public void Setup()
        {
            Random rnd = new Random();
            _pairs = new HashSet<EdgeCollapse>();
            for (int i = 0; i < PairCount; i++)
            {
                _pairs.Add(new EdgeCollapse(i, i + PairCount) { error = rnd.NextDouble() });
            }
        }

        [Benchmark]
        public IReadOnlyCollection<EdgeCollapse> OrderByCustom()
        {
            return new LinkedHashSet<EdgeCollapse>(_pairs.OrderByCustom(x => x).Take(MinsCount));
        }

        [Benchmark]
        public IReadOnlyCollection<EdgeCollapse> OrderBy()
        {
            return new LinkedHashSet<EdgeCollapse>(_pairs.OrderBy(x => x).Take(MinsCount));
        }

        [Benchmark]
        public IReadOnlyCollection<EdgeCollapse> Bruteforce()
        {
            var mins = new LinkedHashSet<EdgeCollapse>();
            mins.Add(_pairs.First());
            foreach (var pair in _pairs)
            {
                if (mins.Count < MinsCount)
                {
                    mins.AddMin(pair);
                }
                else
                {
                    mins.PushMin(pair);
                }
            }
            return mins;
        }
    }
}