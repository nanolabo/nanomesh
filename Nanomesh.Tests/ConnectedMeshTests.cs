using NUnit.Framework;

namespace Nanomesh.Tests
{
    public class ConnectedMeshTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        [Category("ConnectedMesh")]
        public void GetSiblings()
        {
            ConnectedMesh mesh = PrimitiveUtils.CreatePlane(10, 10).ToConnectedMesh();

            var positionToNode = mesh.GetPositionToNode();

            Assert.AreEqual(0, mesh.GetSiblingsCount(positionToNode[0]));
            Assert.AreEqual(2, mesh.GetSiblingsCount(positionToNode[1]));
            Assert.AreEqual(1, mesh.GetSiblingsCount(positionToNode[10]));
            Assert.AreEqual(2, mesh.GetSiblingsCount(positionToNode[11]));
            Assert.AreEqual(5, mesh.GetSiblingsCount(positionToNode[12]));
        }

        [Test]
        [Category("ConnectedMesh")]
        public void EdgeCollapse()
        {
            ConnectedMesh mesh = PrimitiveUtils.CreatePlane(10, 10).ToConnectedMesh();

            var positionToNode = mesh.GetPositionToNode();
            mesh.CollapseEdge(positionToNode[15], positionToNode[16]);
        }

        [Test]
        [Category("ConnectedMesh")]
        public void GetEdgeCount()
        {
            ConnectedMesh mesh = PrimitiveUtils.CreatePlane(10, 10).ToConnectedMesh();

            Assert.IsTrue(mesh.GetEdgeCount(0) == 3);
        }

        [Test]
        [Category("ConnectedMesh")]
        public void AreNodesConnected()
        {
            ConnectedMesh mesh = PrimitiveUtils.CreatePlane(10, 10).ToConnectedMesh();

            Assert.IsFalse(mesh.AreNodesSiblings(0, 1));
            Assert.IsFalse(mesh.AreNodesSiblings(1, 2));
        }
    }
}