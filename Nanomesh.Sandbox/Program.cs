using System;
using System.Diagnostics;
using System.IO;

namespace Nanomesh.Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            //Benchmark();
            
            DecimateFile();

            Console.WriteLine("Done !");
            Console.ReadKey();
        }

        static void DecimateFile()
        {
            SharedMesh sharedMesh = ImporterOBJ.Read(@"..\..\..\..\Nanomesh.Tests\test-models\buggy.obj");
            //sharedMesh.groups = new Group[3]
            //{
            //    new Group { firstIndex = 0, indexCount = 9000 },
            //    new Group { firstIndex = 9000, indexCount = 1000 },
            //    new Group { firstIndex = 10000, indexCount = sharedMesh.triangles.Length - 10000 }
            //};
            ConnectedMesh mesh = ConnectedMesh.Build(sharedMesh);
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere(1, 8));
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(3, 3));

            mesh.MergePositions(0.001);

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            //NormalsModifier normalsModifier = new NormalsModifier();
            //normalsModifier.Run(mesh, 180f);

            Profiling.Start("Decimating");
            DecimateModifier decimateModifier = new DecimateModifier();
            //decimateModifier.DecimateToError(mesh, 0);
            decimateModifier.DecimateToRatio(mesh, 0.2f);
            //decimateModifier.DecimateToPolycount(mesh, 406543);
            //decimateModifier.DecimateToPolycount(mesh, 5000);
            Console.WriteLine(Profiling.End("Decimating"));

            //mesh.Compact();

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            Directory.CreateDirectory(@"..\..\..\..\Nanomesh.Tests\output\");
            ExporterOBJ.Save(mesh.ToSharedMesh(), @"..\..\..\..\Nanomesh.Tests\output\decimation.obj");
        }

        static void Benchmark()
        {
            SharedMesh sharedMesh = PrimitiveUtils.CreateIcoSphere(1, 7);
            ConnectedMesh mesh = ConnectedMesh.Build(sharedMesh);

            NormalsModifier normals = new NormalsModifier();
            normals.Run(mesh, 30);

            Stopwatch sw = Stopwatch.StartNew();

            double ms = Profiling.Time(() => {
                DecimateModifier decimateModifier = new DecimateModifier();
                decimateModifier.DecimateToPolycount(mesh, 500);
            }).TotalMilliseconds;

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            ExporterOBJ.Save(mesh.ToSharedMesh(), Environment.ExpandEnvironmentVariables(@"..\..\..\..\Nanomesh.Tests\output\decimation.obj"));
        }
    }
}