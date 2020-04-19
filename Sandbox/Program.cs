using Nanolabo;
using System;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Profiling.Start("Building Sphere");

            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere());
            mesh.Check();

            Profiling.End("Building Sphere");

            Profiling.Start("Decimating");

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Run(mesh, 0.2f);

            Profiling.End("Decimating");

            mesh.Check();

            SharedMesh smesh = mesh.ToSharedMesh();

            string path = @"C:\Users\OlivierGiniaux\Downloads\decimation.obj";
            ExporterOBJ.Save(smesh, path);

            Console.WriteLine("Done !");
            Console.ReadKey();
        }
    }
}
