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
        [Category("Export")]
        public void ExportOBJ()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(3, 3));

            var positionToNode = mesh.GetPositionToNode();
            mesh.CollapseEdge(positionToNode[9], positionToNode[10], (mesh.positions[9] + mesh.positions[10]) / 2);

            mesh.Check();

            SharedMesh smesh = mesh.ToSharedMesh();

            string path = @"C:\Users\OlivierGiniaux\Downloads\test.obj";
            ExporterOBJ.Save(smesh, path);

            Assert.IsTrue(File.Exists(path));
        }
    }
}