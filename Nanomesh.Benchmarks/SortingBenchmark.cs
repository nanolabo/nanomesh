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
    [SimpleJob(launchCount: 1, warmupCount: 2, targetCount: 5)]
    public class SortingBenchmark
    {
        //[Params(true, false)]
        //public bool Precise { get; set; }

        //[Params(10, 100, 500, 1000, 2000, 5000, 10000)]
        //public int MinsCount { get; set; }

        [Params(10_000, 100_000, 1_000_000)]
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
        public IEnumerable<EdgeCollapse> MinHeap()
        {
            MinHeap<EdgeCollapse> minHeap = new MinHeap<EdgeCollapse>();
            foreach (var pair in _pairs)
            {
                minHeap.Add(pair);
            }
            return minHeap;
        }

        [Benchmark]
        public IEnumerable<float> RadixCustom()
        {
            //GC.TryStartNoGCRegion(1000000000);
            SortedSetRadix32 radixCustom = new RadixSortedSet();
            foreach (var pair in _pairs)
            {
                radixCustom.Add((float)pair.error);
            }
            //GC.EndNoGCRegion();
            return radixCustom;
        }

        [Benchmark]
        public IReadOnlyCollection<EdgeCollapse> Custom()
        {
            return _pairs.OrderByCustom(x => x).ToArray();
        }

        [Benchmark]
        public IReadOnlyCollection<EdgeCollapse> Linq()
        {
            return _pairs.OrderBy(x => x).ToArray();
        }

        [Benchmark]
        public IReadOnlyCollection<EdgeCollapse> LinqTakeHalf()
        {
            return _pairs.OrderBy(x => x).Take(PairCount / 2).ToArray();
        }
    }
}