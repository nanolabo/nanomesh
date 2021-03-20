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
    public class CastBenchmark
    {
        private Vector3List _1;
        private IAttributeList _2;
        private object _3;

        [GlobalSetup]
        public void Setup()
        {
            _1 = new Vector3List(1);
            _2 = _1;
            _3 = _1;
        }

        [Benchmark]
        public Vector3List NoCast()
        {
            return _1;
        }

        [Benchmark]
        public Vector3List OnInterface()
        {
            return (Vector3List)_2;
        }

        [Benchmark]
        public Vector3List OnObject()
        {
            return (Vector3List)_3;
        }
    }
}