using NUnit.Framework;

namespace Nanomesh.Tests
{
    public class DecimationTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Monkey()
        {
            ConnectedMesh mesh = ImporterOBJ.Read(@"../../../test-models/monkey.obj").ToConnectedMesh();

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Initialize(mesh);
            decimateModifier.DecimateToRatio(0.2f);

            Assert.IsTrue(mesh.Check());
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Trisoup()
        {
            ConnectedMesh mesh = ImporterOBJ.Read(@"../../../test-models/trisoup.obj").ToConnectedMesh();

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Initialize(mesh);
            decimateModifier.DecimateToRatio(0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Cube()
        {
            ConnectedMesh mesh = ImporterOBJ.Read(@"../../../test-models/cube.obj").ToConnectedMesh();

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Initialize(mesh);
            decimateModifier.DecimateToRatio(0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Plane()
        {
            ConnectedMesh mesh = ImporterOBJ.Read(@"../../../test-models/plane.obj").ToConnectedMesh();

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Initialize(mesh);
            decimateModifier.DecimateToError(0.01f);

            ExporterOBJ.SaveToFile(mesh.ToSharedMesh(), @"..\..\..\..\Nanomesh.Tests\output\decimation.obj");

            Assert.IsTrue(mesh.Check());
            Assert.AreEqual(2, mesh.FaceCount);
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Star()
        {
            ConnectedMesh mesh = ImporterOBJ.Read(@"../../../test-models/star.obj").ToConnectedMesh();

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Initialize(mesh);
            decimateModifier.DecimateToRatio(0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }
        
        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Hourglass()
        {
            ConnectedMesh mesh = ImporterOBJ.Read(@"../../../test-models/hourglass.obj").ToConnectedMesh();

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.Initialize(mesh);
            decimateModifier.DecimateToRatio(0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }
    }
}