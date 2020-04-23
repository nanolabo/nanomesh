using Nanolabo;
using System;
using System.Diagnostics;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            //DecimateSphere();
            DecimateOBJ();

            Console.WriteLine("Done !");
            Console.ReadKey();
        }

        static void DecimateOBJ()
        {
            Profiling.Start("Reading OBJ");
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"..\..\..\..\Tests\test-models\bunny.obj"));
            Profiling.End("Reading OBJ");

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\original2.obj");

            Debug.Assert(mesh.Check());
            Console.WriteLine("Polycount : " + mesh.FaceCount);

            Profiling.Start("Decimating");
            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Run(mesh, 0.65f);
            Profiling.End("Decimating");

            Debug.Assert(mesh.Check());
            Console.WriteLine("Polycount : " + mesh.FaceCount);

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\decimation.obj");
        }

        static void DecimateSphere()
        {
            Profiling.Start("Building Sphere");
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere(1, 4));
            Profiling.End("Building Sphere");

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\original2.obj");

            Debug.Assert(mesh.Check());
            Console.WriteLine("Polycount : " + mesh.FaceCount);

            Profiling.Start("Decimating");
            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Run(mesh, 0.2f);
            Profiling.End("Decimating");

            Debug.Assert(mesh.Check());
            Console.WriteLine("Polycount : " + mesh.FaceCount);

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"C:\Users\OlivierGiniaux\Downloads\decimation.obj");
        }
    }
}