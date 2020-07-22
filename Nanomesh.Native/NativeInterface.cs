using Nanolabo;
using System;
using System.Runtime.InteropServices;

namespace Nanomesh.Native
{
    public class NativeInterface
    {
        [UnmanagedCallersOnly(EntryPoint = "OptimizeBrake", CallingConvention = CallingConvention.StdCall)]
        public static int OptimizeBrake()
        {
            SharedMesh sharedMesh = ImporterOBJ.Read(@"C:\Users\OlivierGiniaux\Projects\nanolabo\Tests\test-models\brake.obj");
            ConnectedMesh mesh = ConnectedMesh.Build(sharedMesh);
            mesh.MergePositions(0.0001);
            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.DecimateToPolycount(mesh, 120000);
            Console.WriteLine(Profiling.End("Decimating"));
            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\decimation.obj");

            return 1;
        }
    }
}

namespace System.Runtime.InteropServices
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public string EntryPoint;
        public CallingConvention CallingConvention;
        public UnmanagedCallersOnlyAttribute() { }
    }
}
