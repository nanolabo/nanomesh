using BenchmarkDotNet.Attributes;
namespace Nanomesh.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 2, targetCount: 5)]
    public class IteratingBenchmark
    {
        private ConnectedMesh _mesh;

        [GlobalSetup]
        public void Setup()
        {
            _mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));
        }

        [Benchmark]
        public int While()
        {
            int k = 0;
            int relative = 0;
            while ((relative = _mesh.nodes[relative].relative) != 0)
            {
                k++;
            }
            return k;
        }

        [Benchmark]
        public int For()
        {
            int k = 0;
            for (int relative = 0; (relative = _mesh.nodes[relative].relative) != 0;)
            {
                k++;
            }
            return k;
        }
    }
}