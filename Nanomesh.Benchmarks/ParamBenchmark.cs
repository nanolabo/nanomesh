using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
namespace Nanomesh.Benchmarks
{
    [MemoryDiagnoser]
    //[MonoJob("Mono x64", @"C:\Program Files\Mono\bin\mono.exe")]
    //[SimpleJob(RuntimeMoniker.Mono, launchCount: 1, warmupCount: 2, targetCount: 5)]
    //[SimpleJob(RuntimeMoniker.CoreRt31, launchCount: 1, warmupCount: 2, targetCount: 5)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp50, launchCount: 1, warmupCount: 2, targetCount: 5)]
    [SimpleJob(launchCount: 1, warmupCount: 2, targetCount: 5)]
    public class ParamBenchmark
    {
        [Benchmark]
        public Vector3 IntIn()
        {
            Vector3 A = vectors[0];
            Vector3 B = vectors[1];
            return SumIn(in A, in B);
        }

        [Benchmark]
        public Vector3 IntNoIn()
        {
            Vector3 A = vectors[0];
            Vector3 B = vectors[1];
            return SumIn(A, B);
        }

        [Benchmark(Baseline = true)]
        public Vector3 Int()
        {
            Vector3 A = vectors[0];
            Vector3 B = vectors[1];
            return Sum(A, B);
        }

        public Vector3[] vectors = new Vector3[2];

        [Benchmark]
        public Vector3 IntRef()
        {
            ref Vector3 A = ref vectors[0];
            ref Vector3 B = ref vectors[1];
            return SumRef(ref A, ref B);
        }

        public Vector3 SumRef(ref Vector3 A, ref Vector3 B)
        {
            return A + B;
        }

        public Vector3 SumIn(in Vector3 A, in Vector3 B)
        {
            return A + B;
        }

        public Vector3 Sum(Vector3 A, Vector3 B)
        {
            return A + B;
        }
    }
}