using Nanolabo;
using System;
using System.Diagnostics;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Benchmark();

            Console.WriteLine("Done !");
            Console.ReadKey();
        }

        static void DecimateFile()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"..\..\..\..\Tests\test-models\alien.obj"));
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere(1, 4));
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(3, 3));
            Debug.Assert(mesh.Check());

            mesh.MergePositions(0.001);

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            //NormalsModifier normalsModifier = new NormalsModifier();
            //normalsModifier.Run(mesh, 40f);

            Profiling.Start("Decimating");
            DecimateModifier decimateModifier = new DecimateModifier();
            //decimateModifier.DecimateToError(mesh, 0);
            //decimateModifier.DecimatePolycount(mesh, 1);
            //decimateModifier.DecimateToPolycount(mesh, 406543);
            decimateModifier.DecimateToPolycount(mesh, 500);
            Console.WriteLine(Profiling.End("Decimating"));

            //mesh.Compact();

            Debug.Assert(mesh.Check());

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\decimation.obj");
        }

        static void Benchmark()
        {
            Console.WriteLine(DecimateModifier.Benchmark());
        }
    }
}