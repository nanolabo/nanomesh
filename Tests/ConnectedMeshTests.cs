using NUnit.Framework;
using System;
using System.Linq;

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

            Assert.AreEqual(1, mesh.GetSiblings(positionToNode[0]).Count());
            Assert.AreEqual(3, mesh.GetSiblings(positionToNode[1]).Count());
            Assert.AreEqual(2, mesh.GetSiblings(positionToNode[10]).Count());
            Assert.AreEqual(3, mesh.GetSiblings(positionToNode[11]).Count());
            Assert.AreEqual(6, mesh.GetSiblings(positionToNode[12]).Count());
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

            Assert.IsFalse(mesh.AreNodesConnected(0, 1));
            Assert.IsFalse(mesh.AreNodesConnected(1, 2));
        }

        [Test]
        public void Performances()
        {
            Assert.That(Profiling.Time(() =>
            {

            }), Is.LessThanOrEqualTo(TimeSpan.FromSeconds(0)));
        }
    }
}