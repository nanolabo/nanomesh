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

        public bool AreNodesConnected(int nodeIndexA, int nodeIndexB)
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
                    if (AreNodesConnected(nextNodeIndexA, nextNodeIndexB))
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

        public void CollapseEdge(int nodeIndexA, int nodeIndexB)
        {
            //if (!IsAnEdge(nodeIndexA, nodeIndexB))
            //    throw new Exception("Given nodes doesn't make an edge !");

            // For each A sibling, get face.
            //     If face is connected to B
            //         If face is a triangle, rebuild siblings at C without that face node at C
            //     Otherwise 

            foreach (var siblingA in GetSiblings(nodeIndexA))
            {
                bool isFaceTouched = false;
                int faceEdgeCount = 0;
                int nodeIndexC = -1;
                foreach (var nextNode in GetFaceNodes(siblingA))
                {
                    if (nextNode == nodeIndexB)
                    {
                        isFaceTouched = true;
                    }
                    else if (nextNode != nodeIndexA)
                    {
                        nodeIndexC = nextNode;
                    }
                    faceEdgeCount++;
                }
                if (isFaceTouched && faceEdgeCount == 3)
                {
                    int previousSiblingC = -1;
                    int firstSiblingC = -1;
                    foreach (var siblingC in GetSiblings(nodeIndexC).ToArray())
                    {
                        if (previousSiblingC != -1)
                        {
                            nodes[siblingC].nextSibling = previousSiblingC;
                        }
                        else
                        {
                            firstSiblingC = siblingC;
                        }
                        previousSiblingC = siblingC;
                    }
                    nodes[firstSiblingC].nextSibling = previousSiblingC;
                }
            }

            int previousSiblingA = -1;
            int firstSiblingA = -1;

            foreach (var siblingA in GetSiblings(nodeIndexA).ToArray())
            {
                bool isFaceTouched = false;
                int nodeIndexC = -1;
                foreach (var nextNode in GetFaceNodes(siblingA))
                {
                    if (nextNode == nodeIndexB)
                    {
                        isFaceTouched = true;
                    }
                    else if (nextNode != nodeIndexA)
                    {
                        nodeIndexC = nextNode;
                    }
                }

                if (!isFaceTouched)
                {
                    if (previousSiblingA != -1)
                    {
                        nodes[siblingA].nextSibling = previousSiblingA;
                    }
                    else
                    {
                        firstSiblingA = siblingA;
                    }
                    previousSiblingA = siblingA;
                }
            }
            nodes[firstSiblingA].nextSibling = previousSiblingA;

            int previousSiblingB = -1;
            int firstSiblingB = -1;

            foreach (var siblingB in GetSiblings(nodeIndexB).ToArray())
            {
                bool isFaceTouched = false;
                int nodeIndexC = -1;
                foreach (var nextNode in GetFaceNodes(siblingB))
                {
                    if (nextNode == nodeIndexB)
                    {
                        isFaceTouched = true;
                    }
                    else if (nextNode != nodeIndexA)
                    {
                        nodeIndexC = nextNode;
                    }
                }

                if (!isFaceTouched)
                {
                    if (previousSiblingB != -1)
                    {
                        nodes[siblingB].nextSibling = previousSiblingB;
                    }
                    else
                    {
                        firstSiblingB = siblingB;
                    }
                    previousSiblingB = siblingB;
                }
            }
            nodes[firstSiblingB].nextSibling = previousSiblingB;
        }
    }
}