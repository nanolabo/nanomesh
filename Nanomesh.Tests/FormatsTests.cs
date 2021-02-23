using NUnit.Framework;
using System.IO;

namespace Nanomesh.Tests
{
    public class FormatsTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        [Category("Export")]
        public void ExportOBJ()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(3, 3));

            mesh.Check();

            SharedMesh smesh = mesh.ToSharedMesh();

            string path = @"../../../../Nanomesh.Tests/output/export-obj.obj";
            ExporterOBJ.Save(smesh, path);

            Assert.IsTrue(File.Exists(path));
        }
    }
}