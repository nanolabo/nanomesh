using Nanomesh;
using System.Runtime.InteropServices;

namespace Nanomesh.Native
{
    public class NativeInterfaceX
    {
        [UnmanagedCallersOnly(EntryPoint = "Benchmark")]
        public static double Benchmark()
        {
            return DecimateModifier.Benchmark();
        }
    }
}
