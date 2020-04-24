using Nanolabo;
using System;
using System.Diagnostics;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"..\..\..\..\Tests\test-models\monkey.obj"));
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere(1, 4));
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(3, 3));
            Debug.Assert(mesh.Check());

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            Profiling.Start("Decimating");
            DecimateModifier decimateModifier = new DecimateModifier();
            //decimateModifier.DecimatePolycount(mesh, 3);
            decimateModifier.DecimateToRatio(mesh, 0.5f);
            Profiling.End("Decimating");
            Debug.Assert(mesh.Check());

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\decimation.obj");

            Console.WriteLine("Done !");
            Console.ReadKey();
        }
    }
}