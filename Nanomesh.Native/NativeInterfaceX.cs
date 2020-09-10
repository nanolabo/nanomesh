using Nanolabo;
using System.Runtime.InteropServices;

namespace Nanomesh.Native
{
    public class NativeInterfaceX
    {
        [UnmanagedCallersOnly(EntryPoint = "Benchmark")]
        public static int Benchmark()
        {
            DecimateModifier.Benchmark();
            return 123;
        }
    }
}
