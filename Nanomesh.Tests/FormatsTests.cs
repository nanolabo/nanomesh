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
            ConnectedMesh mesh = PrimitiveUtils.CreatePlane(3, 3).ToConnectedMesh();

            mesh.Check();

            SharedMesh smesh = mesh.ToSharedMesh();

            using (MemoryStream ms = new MemoryStream())
            {
                ExporterOBJ.SaveToStream(smesh, ms);

                Assert.IsTrue(ms.Length > 0);
            }
        }
    }
}