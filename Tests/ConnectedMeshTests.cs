using NUnit.Framework;

namespace Nanolabo
{
    public class ConnectedMeshTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void GetSiblings()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            var positionToNode = mesh.GetPositionToNode();

            Assert.AreEqual(0, mesh.GetSiblingsCount(positionToNode[0]));
            Assert.AreEqual(2, mesh.GetSiblingsCount(positionToNode[1]));
            Assert.AreEqual(1, mesh.GetSiblingsCount(positionToNode[10]));
            Assert.AreEqual(2, mesh.GetSiblingsCount(positionToNode[11]));
            Assert.AreEqual(5, mesh.GetSiblingsCount(positionToNode[12]));
        }

        [Test]
        public void EdgeCollapse()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            var positionToNode = mesh.GetPositionToNode();
            mesh.CollapseEdge(positionToNode[15], positionToNode[16]);
        }

        [Test]
        public void GetEdgeCount()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            Assert.IsTrue(mesh.GetEdgeCount(0) == 3);
        }
        
        [Test]
        public void AreNodesConnected()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            Assert.IsFalse(mesh.AreNodesSiblings(0, 1));
            Assert.IsFalse(mesh.AreNodesSiblings(1, 2));
        }
    }
}