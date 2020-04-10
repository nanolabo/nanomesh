using NUnit.Framework;
using System.IO;

namespace Nanolabo
{
    public class FormatsTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ExportOBJ()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            var positionToNode = mesh.GetPositionToNode();
            mesh.CollapseEdge(positionToNode[15], positionToNode[16]);

            SharedMesh smesh = mesh.ToSharedMesh();

            string path = @"C:\Users\OlivierGiniaux\Downloads\test.obj";
            ExporterOBJ.Save(smesh, path);

            Assert.IsTrue(File.Exists(path));
        }
    }
}