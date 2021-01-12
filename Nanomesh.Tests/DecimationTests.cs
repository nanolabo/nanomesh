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
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"../../../test-models/monkey.obj"));

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.DecimateToRatio(mesh, 0.2f);

            Assert.IsTrue(mesh.Check());
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Trisoup()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"../../../test-models/trisoup.obj"));

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.DecimateToRatio(mesh, 0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Cube()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"../../../test-models/cube.obj"));

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.DecimateToRatio(mesh, 0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Plane()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"../../../test-models/plane.obj"));

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.DecimateToError(mesh, 0.01f);

            ExporterOBJ.Save(mesh.ToSharedMesh(), @"..\..\..\..\Nanomesh.Tests\output\decimation.obj");

            Assert.IsTrue(mesh.Check());
            Assert.AreEqual(2, mesh.FaceCount);
        }

        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Star()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"../../../test-models/star.obj"));

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.DecimateToRatio(mesh, 0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }
        
        [Test]
        [Category("Decimate")]
        [Category("Import")]
        public void Hourglass()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(ImporterOBJ.Read(@"../../../test-models/hourglass.obj"));

            Assert.IsTrue(mesh.Check());

            DecimateModifier decimateModifier = new DecimateModifier();
            decimateModifier.DecimateToRatio(mesh, 0f);

            Assert.IsTrue(mesh.Check());
            Assert.IsTrue(mesh.FaceCount == 0);
        }
    }
}