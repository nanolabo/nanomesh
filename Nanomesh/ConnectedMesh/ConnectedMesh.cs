using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Experimental = System.ObsoleteAttribute;

namespace Nanolabo
{
    // Let's say F = 2V
    // Halfedge mesh is V * sizeof(vertex) + 3F * sizeof(Halfedge) + F * sizeof(Face) = 16 * 0.5F + 3F * 20 + 4F = 72F
    // Connected mesh is V * sizeof(Vector3) + 3F * sizeof(Node) + F * sizeof(Face) = 12 * 0.5F + 3F * 12 + 12F = 54F (without attributes)
    // Connected mesh no face is V * sizeof(Vector3) + 3F * sizeof(Node) = 12 * 0.5F + 3F * 12 = 42F (without attributes)

    public partial class ConnectedMesh
    {
        public Vector3[] positions;
        public Attribute[] attributes;
        public Node[] nodes;

        private int faceCount;
        public int FaceCount => faceCount;

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

                A.relative = nodesList.Count + 1; // B
                B.relative = nodesList.Count + 2; // C
                C.relative = nodesList.Count; // A

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

                connectedMesh.faceCount++;
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
                        connectedMesh.nodes[node].sibling = previousSibling;
                    }
                    else
                    {
                        firstSibling = node;
                    }
                    previousSibling = node;
                }
                connectedMesh.nodes[firstSibling].sibling = previousSibling;
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
                if (browsedNodes.Contains(i) || nodes[i].IsRemoved())
                    continue;

                // Only works if all elements are triangles
                int relative = i;
                do
                {
                    browsedNodes.Add(relative);
                    triangles.Add(nodes[relative].position);
                } while ((relative = nodes[relative].relative) != i);
            }

            mesh.vertices = positions;
            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        // inline ?
        public bool AreNodesSiblings(int nodeIndexA, int nodeIndexB)
        {
            return nodes[nodeIndexA].position == nodes[nodeIndexB].position;
        }

        public int[] GetPositionToNode()
        {
            int[] positionToNode = new int[positions.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved())
                    positionToNode[nodes[i].position] = i;
            }
            return positionToNode;
        }

        [Experimental]
        public bool IsAnEdge(int nodeIndexA, int nodeIndexB)
        {
            // Giflé
            return nodes[nodes[nodeIndexA].relative].position == nodes[nodeIndexB].relative
                || nodes[nodes[nodeIndexB].relative].position == nodes[nodeIndexA].relative;
        }

        public int GetEdgeCount(int nodeIndex)
        {
            return GetRelativesCount(nodeIndex) + 1;
        }

        public int GetRelativesCount(int nodeIndex)
        {
            int k = 0;
            int relative = nodeIndex;
            while ((relative = nodes[relative].relative) != nodeIndex)
            {
                k++;
            }
            return k;
        }

        [Obsolete]
        public int[] GetRelatives(int nodeIndex)
        {
            // Make room
            int k = 0;
            int relative = nodeIndex;
            do
            {
                k++;
            } while ((relative = nodes[relative].relative) != nodeIndex);

            // Fill
            int[] res = new int[k];
            k = 0;
            do
            {
                res[k++] = relative;
            } while ((relative = nodes[relative].relative) != nodeIndex);

            return res;
        }

        public int GetSiblingsCount(int nodeIndex)
        {
            int k = 0;
            int sibling = nodeIndex;
            while ((sibling = nodes[sibling].sibling) != nodeIndex)
            {
                k++;
            }
            return k;
        }

        [Obsolete]
        public int[] GetSiblings(int nodeIndex)
        {
            // Make room
            int k = 0;
            int sibling = nodeIndex;
            do
            {
                k++;
            } while ((sibling = nodes[sibling].sibling) != nodeIndex);

            // Fill
            int[] res = new int[k];
            k = 0;
            do
            {
                res[k++] = sibling;
            } while ((sibling = nodes[sibling].sibling) != nodeIndex);

            return res;
        }

        public void ReconnectSiblings(int nodeIndex)
        {
            int sibling = nodeIndex;
            int lastValid = -1;
            int firstValid = -1;
            int position = -1;

            do
            {
                if (nodes[sibling].IsRemoved())
                    continue;

                if (firstValid == -1)
                {
                    firstValid = sibling;
                    position = nodes[sibling].position;
                }

                if (lastValid != -1)
                {
                    nodes[lastValid].sibling = sibling;
                    nodes[lastValid].position = position;
                }

                lastValid = sibling;
            }
            while ((sibling = nodes[sibling].sibling) != nodeIndex);

            // Close the loop
            nodes[lastValid].sibling = firstValid; // Additional checks here ?
            nodes[lastValid].position = position;
        }

        public void ReconnectSiblings(int nodeIndexA, int nodeIndexB)
        {
            int sibling = nodeIndexA;
            int lastValid = -1;
            int firstValid = -1;
            int position = -1;

            do
            {
                if (nodes[sibling].IsRemoved())
                    continue;

                if (firstValid == -1)
                {
                    firstValid = sibling;
                    position = nodes[sibling].position;
                }

                if (lastValid != -1)
                {
                    nodes[lastValid].sibling = sibling;
                    nodes[lastValid].position = position;
                }

                lastValid = sibling;
            }
            while ((sibling = nodes[sibling].sibling) != nodeIndexA);

            sibling = nodeIndexB;
            do
            {
                if (nodes[sibling].IsRemoved())
                    continue;

                if (firstValid == -1)
                {
                    firstValid = sibling;
                    position = nodes[sibling].position;
                }

                if (lastValid != -1)
                {
                    nodes[lastValid].sibling = sibling;
                    nodes[lastValid].position = position;
                }

                lastValid = sibling;
            }
            while ((sibling = nodes[sibling].sibling) != nodeIndexB);

            // Close the loop
            nodes[lastValid].sibling = firstValid; // Additional checks here ?
            nodes[lastValid].position = position;
        }

        public void CollapseEdge(int nodeIndexA, int nodeIndexB, Vector3 position)
        {
            int posA = nodes[nodeIndexA].position;
            int posB = nodes[nodeIndexB].position;

            Debug.Assert(posA != posB, "A and B must have different positions");
            Debug.Assert(!nodes[nodeIndexA].IsRemoved());
            Debug.Assert(!nodes[nodeIndexB].IsRemoved());

            Debug.Assert(CheckRelatives(nodeIndexA), "A's relatives must be valid");
            Debug.Assert(CheckRelatives(nodeIndexB), "B's relatives must be valid");
            Debug.Assert(CheckSiblings(nodeIndexA), "A's siblings must be valid");
            Debug.Assert(CheckSiblings(nodeIndexB), "B's siblings must be valid");

            positions[nodes[nodeIndexA].position] = position;

            int siblingOfA = nodeIndexA;
            do // Iterate over faces adjacent to node A
            {
                bool isFaceTouched = false;
                int faceEdgeCount = 0;
                int nodeIndexC = -1;

                int relativeOfA = siblingOfA;
                do // Iterate over adjacent face nodes
                {
                    int posC = nodes[relativeOfA].position;
                    if (posC == posB)
                    {
                        isFaceTouched = true;
                    }
                    else if (posC != posA)
                    {
                        nodeIndexC = relativeOfA;
                    }

                    faceEdgeCount++;
                } while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA);

                if (isFaceTouched && faceEdgeCount == 3)
                {
                    relativeOfA = siblingOfA;
                    do
                    {
                        nodes[relativeOfA].MarkRemoved();
                    } while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA);

                    ReconnectSiblings(nodeIndexC);

                    faceCount--;

                    Debug.Assert(CheckRelatives(nodeIndexB), "C's relatives must be valid");
                    Debug.Assert(CheckSiblings(nodeIndexA), "C's siblings must be valid");
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            ReconnectSiblings(nodeIndexA, nodeIndexB);

            Debug.Assert(CheckRelatives(nodeIndexA), "A's relatives must be valid");
            Debug.Assert(CheckRelatives(nodeIndexB), "B's relatives must be valid");
            Debug.Assert(CheckSiblings(nodeIndexA), "A's siblings must be valid");
            Debug.Assert(CheckSiblings(nodeIndexB), "B's siblings must be valid");
        }

        public void Compact()
        {
            Dictionary<int, int> oldToNewNodeIndex = new Dictionary<int, int>();
            int validNodesCount = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved())
                {
                    validNodesCount++;
                }
            }

            Node[] newNodes = new Node[validNodesCount];

            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved())
                {
                    newNodes[oldToNewNodeIndex.Count] = nodes[i];
                    oldToNewNodeIndex.Add(i, oldToNewNodeIndex.Count);
                }
            }

            for (int i = 0; i < newNodes.Length; i++)
            {
                newNodes[i].relative = oldToNewNodeIndex[newNodes[i].relative];
                newNodes[i].sibling = oldToNewNodeIndex[newNodes[i].sibling];
            }

            nodes = newNodes;

            // Todo : compact positions and attributes

            GC.Collect(); // Collect 
        }
    }
}