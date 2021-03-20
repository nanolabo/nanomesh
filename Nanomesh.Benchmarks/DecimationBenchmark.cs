using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Nanomesh.Benchmarks
{
    [MemoryDiagnoser]
    //[MonoJob("Mono x64", @"C:\Program Files\Mono\bin\mono.exe")]
    //[SimpleJob(RuntimeMoniker.Mono, launchCount: 1, warmupCount: 2, targetCount: 5)]
    //[SimpleJob(RuntimeMoniker.CoreRt31, launchCount: 1, warmupCount: 2, targetCount: 5)]
    [SimpleJob(launchCount: 1, warmupCount: 2, targetCount: 5)]
    public class DecimationBenchmark
    {
        [Params(true, false)]
        public bool Precise { get; set; }

        private ConnectedMesh _mesh;

        [IterationSetup]
        public void IterationSetup()
        {
            _mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere(1, 4));
            _mesh.MergePositions(0.001);
        }

        [Benchmark]
        public void DecimateSphere()
        {
            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.UpdateFarNeighbors = Precise;
            decimateModifier.DecimateToPolycount(_mesh, 500);
        }
    }
}