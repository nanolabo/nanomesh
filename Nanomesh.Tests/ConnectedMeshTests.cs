using NUnit.Framework;
using System.Linq;

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
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

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
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            var positionToNode = mesh.GetPositionToNode();
            mesh.CollapseEdge(positionToNode[15], positionToNode[16]);
        }

        [Test]
        [Category("ConnectedMesh")]
        public void GetEdgeCount()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            Assert.IsTrue(mesh.GetEdgeCount(0) == 3);
        }
        
        [Test]
        [Category("ConnectedMesh")]
        public void AreNodesConnected()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreatePlane(10, 10));

            Assert.IsFalse(mesh.AreNodesSiblings(0, 1));
            Assert.IsFalse(mesh.AreNodesSiblings(1, 2));
        }

        [Test]
        [Category("ConnectedMesh")]
        public void IsEdgeHard()
        {
            ConnectedMesh mesh = ConnectedMesh.Build(PrimitiveUtils.CreateBox());
            mesh.MergePositions();
            NormalsModifier nm = new NormalsModifier();
            nm.Run(mesh, 20);

            var edges = mesh.GetAllEdges();

            foreach (Edge edge in edges)
            {
                Vector3 A = mesh.positions[edge.posA];
                Vector3 B = mesh.positions[edge.posB];

                Assert.Fail("Please fix test");

                if ((B - A).Length == 1)
                {
                    //Assert.AreEqual(EdgeTopology.HardEdge, mesh.GetEdgeTopo(mesh.PositionToNode[edge.posA], mesh.PositionToNode[edge.posB]));
                }
                else
                {
                    //Assert.AreEqual(EdgeTopology.Surface, mesh.GetEdgeTopo(mesh.PositionToNode[edge.posA], mesh.PositionToNode[edge.posB]));
                }
            }
        }
    }
}