using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.Linq;

namespace Nanomesh.Benchmarks
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            //var benchmark = new SortingBenchmark();
            //benchmark.PairCount = 100000;
            //benchmark.Setup();
            //var b = benchmark.Bruteforce();

            ManualConfig conf = new ManualConfig();
            conf.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
            conf.AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
            conf.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
            conf.AddDiagnoser(MemoryDiagnoser.Default);

            var switcher = new BenchmarkSwitcher(new[] {
                typeof(DecimationBenchmark),
                typeof(SortingBenchmark),
                typeof(ParamBenchmark),
            });

            switcher.Run(args, config: conf);

            Console.ReadKey();
        }
    }
}
