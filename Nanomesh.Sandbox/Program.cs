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
            SharedMesh sharedMesh = ImporterOBJ.Read(@"../../../../Nanomesh.Tests/test-models/buggy.obj");
            //sharedMesh.groups = new Group[3]
            //{
            //    new Group { firstIndex = 0, indexCount = 9000 },
            //    new Group { firstIndex = 9000, indexCount = 1000 },
            //    new Group { firstIndex = 10000, indexCount = sharedMesh.triangles.Length - 10000 }
            //};
            ConnectedMesh mesh = sharedMesh.ToConnectedMesh();
            //ConnectedMesh mesh = PrimitiveUtils.CreateIcoSphere(1, 6).ToConnectedMesh();
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(3, 3));

            //mesh.MergePositions(0.001);

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            Profiling.Start("Decimating");
            DecimateModifier decimateModifier = new DecimateModifier();
            //decimateModifier.DecimateToError(mesh, 0);
            decimateModifier.Initialize(mesh);
            decimateModifier.DecimateToRatio(0.5f);
            //decimateModifier.DecimateToPolycount(mesh, 406543);
            //decimateModifier.DecimateToPolycount(mesh, 5000);
            Console.WriteLine(Profiling.End("Decimating"));

            //NormalsModifier normalsModifier = new NormalsModifier();
            //normalsModifier.Run(mesh, 55);

            //mesh.Compact();

            Console.WriteLine("Polycount : " + mesh.FaceCount);

            Directory.CreateDirectory(@"../../../../Nanomesh.Tests/output/");
            ExporterOBJ.SaveToFile(mesh.ToSharedMesh(), @"../../../../Nanomesh.Tests/output/decimation.obj");
        }

        static void Benchmark()
        {
            SharedMesh sharedMesh = PrimitiveUtils.CreateIcoSphere(1, 7);
            ConnectedMesh mesh = sharedMesh.ToConnectedMesh();

            NormalsModifier normals = new NormalsModifier();
            normals.Run(mesh, 30);

            Stopwatch sw = Stopwatch.StartNew();

            double ms = Profiling.Time(() => {
                DecimateModifier decimateModifier = new DecimateModifier();
                decimateModifier.Initialize(mesh);
                decimateModifier.DecimateToPolycount(500);
            }).TotalMilliseconds;

            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds);

            ExporterOBJ.SaveToFile(mesh.ToSharedMesh(), Environment.ExpandEnvironmentVariables(@"..\..\..\..\Nanomesh.Tests\output\decimation.obj"));
        }
    }
}