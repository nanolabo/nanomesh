﻿using Nanomesh;
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
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"..\..\..\..\Nanomesh.Tests\test-models\alien.obj"));
            //ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateIcoSphere(1, 8));
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
            decimateModifier.DecimateToPolycount(mesh, 5000);
            Console.WriteLine(Profiling.End("Decimating"));

            //mesh.Compact();

            Debug.Assert(mesh.Check());

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
                decimateModifier.DecimateToRatio(mesh, 0.50f);
            }).TotalMilliseconds;

            sw.Stop();

            Console.WriteLine(sw);

            ExporterOBJ.Save(mesh.ToSharedMesh(), Environment.ExpandEnvironmentVariables(@"..\..\..\..\Nanomesh.Tests\output\decimation.obj"));
        }
    }
}