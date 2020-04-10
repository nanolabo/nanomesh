using System;
using System.Collections.Generic;
using System.Linq;

namespace Nanolabo
{
    // Let's say F = 2V
    // Halfedge mesh is V * sizeof(vertex) + 3F * sizeof(Halfedge) + F * sizeof(Face) = 16 * 0.5F + 3F * 20 + 4F = 72F
    // Connected mesh is V * sizeof(Vector3) + 3F * sizeof(Node) + F * sizeof(Face) = 12 * 0.5F + 3F * 12 + 12F = 54F (without attributes)
    // Connected mesh no face is V * sizeof(Vector3) + 3F * sizeof(Node) = 12 * 0.5F + 3F * 12 = 42F (without attributes)

    public class ConnectedMesh
    {
        public Vector3[] positions;
        public Attribute[] attributes;
        public Node[] nodes;

        public static ConnectedMesh Build(SharedMesh mesh)
        {
            ConnectedMesh connectedMesh = new ConnectedMesh();

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            connectedMesh.positions = vertices;
            connectedMesh.attributes = new Attribute[vertices.Length];

            List<Node> nodesList = new List<Node>();
            Dictionary<int, List<int>> vertexToNodes = new Dictionary<int, List<int>>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Node A = new Node();
                Node B = new Node();
                Node C = new Node();

                A.position = triangles[i];
                B.position = triangles[i + 1];
                C.position = triangles[i + 2];

                A.attribute = triangles[i];
                B.attribute = triangles[i + 1];
                C.attribute = triangles[i + 2];

                A.nextNode = nodesList.Count + 1; // B
                B.nextNode = nodesList.Count + 2; // C
                C.nextNode = nodesList.Count; // A

                if (!vertexToNodes.ContainsKey(A.position))
                    vertexToNodes.Add(A.position, new List<int>());
                if (!vertexToNodes.ContainsKey(B.position))
                    vertexToNodes.Add(B.position, new List<int>());
                if (!vertexToNodes.ContainsKey(C.position))
                    vertexToNodes.Add(C.position, new List<int>());

                vertexToNodes[A.position].Add(nodesList.Count);
                vertexToNodes[B.position].Add(nodesList.Count + 1);
                vertexToNodes[C.position].Add(nodesList.Count + 2);

                nodesList.Add(A);
                nodesList.Add(B);
                nodesList.Add(C);
            }

            connectedMesh.nodes = nodesList.ToArray();

            foreach (var pair in vertexToNodes)
            {
                int previousSibling = -1;
                int firstSibling = -1;
                foreach (int node in pair.Value)
                {
                    if (firstSibling != -1)
                    {
                        connectedMesh.nodes[node].nextSibling = previousSibling;
                    }
                    else
                    {
                        firstSibling = node;
                    }
                    previousSibling = node;
                }
                connectedMesh.nodes[firstSibling].nextSibling = previousSibling;
            }

            return connectedMesh;
        }

        public SharedMesh ToSharedMesh()
        {
            SharedMesh mesh = new SharedMesh();

            List<int> triangles = new List<int>();

            HashSet<int> browsedNodes = new HashSet<int>();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (browsedNodes.Contains(i) || nodes[i].position < 0)
                    continue;

                // Only works if all elements are triangles
                foreach (var sibling in GetFaceNodes(i))
                {
                    browsedNodes.Add(sibling);

                    triangles.Add(nodes[sibling].position);
                }
            }

            mesh.vertices = positions;
            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        public bool AreNodesSiblings(int nodeIndexA, int nodeIndexB)
        {
            if (nodeIndexA == nodeIndexB)
                throw new Exception("A and B is the name node !");

            ref Node nodeA = ref nodes[nodeIndexA];
            ref Node nodeB = ref nodes[nodeIndexB];

            return nodeA.position == nodeB.position;
        }

        public int[] GetPositionToNode()
        {
            int[] positionToNode = new int[positions.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                positionToNode[nodes[i].position] = i;
            }
            return positionToNode;
        }

        public bool IsAnEdge(int nodeIndexA, int nodeIndexB)
        {
            // Giflé
            return nodes[nodes[nodeIndexA].nextNode].position == nodes[nodeIndexB].nextNode
                || nodes[nodes[nodeIndexB].nextNode].position == nodes[nodeIndexA].nextNode;
        }

        public int GetEdgeCount(int nodeIndex)
        {
            return GetFaceNodes(nodeIndex).Count();
        }

        public bool AreFacesConnected(int nodeIndexA, int nodeIndexB)
        {
            foreach (var nextNodeIndexA in GetFaceNodes(nodeIndexA))
            {
                foreach (var nextNodeIndexB in GetFaceNodes(nodeIndexB))
                {
                    if (AreNodesSiblings(nextNodeIndexA, nextNodeIndexB))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public IEnumerable<int> GetFaceNodes(int nodeIndex)
        {
            int nextNodeIndex = nodeIndex;
            do
            {
                nextNodeIndex = nodes[nextNodeIndex].nextNode;
                yield return nextNodeIndex;
            }
            while (nextNodeIndex != nodeIndex);
        }

        public IEnumerable<int> GetSiblings(int nodeIndex)
        {
            int nextNodeIndex = nodeIndex;
            do
            {
                nextNodeIndex = nodes[nextNodeIndex].nextSibling;
                yield return nextNodeIndex;
            }
            while (nextNodeIndex != nodeIndex);
        }

        public void ReconnectSiblings(IEnumerable<int> siblings)
        {
            int previousSibling = -1;
            int firstSibling = -1;
            int position = -1;

            foreach (var sibling in siblings.ToArray())
            {
                if (nodes[sibling].position < 0)
                    continue;

                if (previousSibling != -1)
                {
                    nodes[sibling].nextSibling = previousSibling;
                    nodes[sibling].position = position;
                }
                else
                {
                    position = nodes[sibling].position;
                    firstSibling = sibling;
                }
                previousSibling = sibling;
            }
            nodes[firstSibling].nextSibling = previousSibling;
        }

        public void CollapseEdge(int nodeIndexA, int nodeIndexB)
        {
            //if (!IsAnEdge(nodeIndexA, nodeIndexB))
            //    throw new Exception("Given nodes doesn't make an edge !");

            positions[nodes[nodeIndexA].position] = (positions[nodes[nodeIndexA].position] + positions[nodes[nodeIndexB].position]) / 2;

            foreach (var siblingA in GetSiblings(nodeIndexA))
            {
                bool isFaceTouched = false;
                int faceEdgeCount = 0;
                int nodeIndexC = -1;

                foreach (var nextNode in GetFaceNodes(siblingA))
                {
                    if (AreNodesSiblings(nextNode, nodeIndexB))
                    {
                        nodes[nextNode].position = -1;
                        nodes[siblingA].position = -1;
                        isFaceTouched = true;
                    }
                    else if (nextNode != siblingA)
                    {
                        nodeIndexC = nextNode;
                    }

                    faceEdgeCount++;
                }

                if (isFaceTouched && faceEdgeCount == 3)
                {
                    nodes[nodeIndexC].position = -1;
                    ReconnectSiblings(GetSiblings(nodeIndexC));
                }
            }

            ReconnectSiblings(GetSiblings(nodeIndexA).Concat(GetSiblings(nodeIndexB)));
        }
    }
}