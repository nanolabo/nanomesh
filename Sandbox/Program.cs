using Nanolabo;
using System;
using System.Diagnostics;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Profiling.Start("Building Sphere");
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere());
            Profiling.End("Building Sphere");

            Debug.Assert(mesh.Check());

            Profiling.Start("Decimating");
            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Run(mesh, 0.2f);
            Profiling.End("Decimating");

            Debug.Assert(mesh.Check());

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\decimation.obj");

            Console.WriteLine("Done !");
            Console.ReadKey();
        }
    }
}