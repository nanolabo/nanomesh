using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public int[] PositionToNode => positionToNode ?? (positionToNode = GetPositionToNode());
        private int[] positionToNode;

        internal int faceCount;
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

            Debug.Assert(connectedMesh.Check());

            return connectedMesh;
        }

        public SharedMesh ToSharedMesh()
        {
            SharedMesh mesh = new SharedMesh();

            List<int> triangles = new List<int>();

            HashSet<int> browsedNodes = new HashSet<int>();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (browsedNodes.Contains(i) || nodes[i].IsRemoved)
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
#if DEBUG
            for (int i = 0; i < positions.Length; i++)
            {
                positionToNode[i] = -1;
            }
#endif
            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved)
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

        public int ReconnectSiblings(int nodeIndex)
        {
            int sibling = nodeIndex;
            int lastValid = -1;
            int firstValid = -1;
            int position = -1;

            do
            {
                if (nodes[sibling].IsRemoved)
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

            if (lastValid == -1)
                return -1; // All siblings were removed

            // Close the loop
            nodes[lastValid].sibling = firstValid;
            nodes[lastValid].position = position;

            return firstValid;
        }

        public int ReconnectSiblings(int nodeIndexA, int nodeIndexB, int position)
        {
            int sibling = nodeIndexA;
            int lastValid = -1;
            int firstValid = -1;

            do
            {
                if (nodes[sibling].IsRemoved)
                    continue;

                if (firstValid == -1)
                {
                    firstValid = sibling;
                    //position = nodes[sibling].position;
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
                if (nodes[sibling].IsRemoved)
                    continue;

                if (firstValid == -1)
                {
                    firstValid = sibling;
                    //position = nodes[sibling].position;
                }

                if (lastValid != -1)
                {
                    nodes[lastValid].sibling = sibling;
                    nodes[lastValid].position = position;
                }

                lastValid = sibling;
            }
            while ((sibling = nodes[sibling].sibling) != nodeIndexB);

            if (lastValid == -1)
                return -1; // All siblings were removed

            // Close the loop
            nodes[lastValid].sibling = firstValid;
            nodes[lastValid].position = position;

            return firstValid;
        }

        public int CollapseEdge(int nodeIndexA, int nodeIndexB, Vector3 position)
        {
            int posA = nodes[nodeIndexA].position;
            int posB = nodes[nodeIndexB].position;

            Debug.Assert(posA != posB, "A and B must have different positions");
            Debug.Assert(!nodes[nodeIndexA].IsRemoved);
            Debug.Assert(!nodes[nodeIndexB].IsRemoved);

            Debug.Assert(CheckRelatives(nodeIndexA), "A's relatives must be valid");
            Debug.Assert(CheckRelatives(nodeIndexB), "B's relatives must be valid");
            Debug.Assert(CheckSiblings(nodeIndexA), "A's siblings must be valid");
            Debug.Assert(CheckSiblings(nodeIndexB), "B's siblings must be valid");

            positions[posA] = position;

            int siblingOfA = nodeIndexA;
            do // Iterator over faces around A
            {
                bool isFaceTouched = false;
                int faceEdgeCount = 0;
                int nodeIndexC = -1;

                int relativeOfA = siblingOfA;
                do // Circulate around face
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
                    // Remove face : Mark nodes as removed an reconnect siblings around C

                    int posC = nodes[nodeIndexC].position;

                    relativeOfA = siblingOfA;
                    do
                    {
                        nodes[relativeOfA].MarkRemoved();

                    } while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA);

                    int validNodeAtC = ReconnectSiblings(nodeIndexC);

                    if (positionToNode != null)
                        positionToNode[posC] = validNodeAtC;

                    faceCount--;
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            int validNodeAtA = ReconnectSiblings(nodeIndexA, nodeIndexB, posA);

            if (positionToNode != null)
            {
                positionToNode[posA] = validNodeAtA;
                positionToNode[posB] = -1;
            }

            return validNodeAtA;
        }

        bool IsEdgeInSurface(int nodeIndexA, int nodeIndexB)
        {
            int posB = nodes[nodeIndexB].position;

            int facesAttached = 0;

            int siblingOfA = nodeIndexA;
            do // Iterator over faces around A
            {
                int relativeOfA = siblingOfA;
                while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA)
                {
                    int posC = nodes[relativeOfA].position;
                    if (posC == posB)
                    {
                        facesAttached++;
                        if (facesAttached == 2)
                            return true;
                    }
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            return false;
        }

        public enum EdgeType
        {
            Unknown,
            Manifold,
            AShape,
            TShapeA,
            TShapeB,
            Border
        }

        public EdgeType GetEdgeType(int nodeIndexA, int nodeIndexB, out int borderNodeA, out int borderNodeB)
        {
            borderNodeA = -1;
            borderNodeB = -1;

            int posA = nodes[nodeIndexA].position;
            int posB = nodes[nodeIndexB].position;

            int sibling = nodeIndexA;
            do
            {
                int relative = sibling;
                while ((relative = nodes[relative].relative) != sibling)
                {
                    int posC = nodes[relative].position;
                    if (posC != posB)
                    {
                        if (!IsEdgeInSurface(sibling, relative))
                        {
                            borderNodeA = relative;
                            goto skipA;
                        }
                    }
                }
            } while ((sibling = nodes[sibling].sibling) != nodeIndexA);

            skipA:;

            sibling = nodeIndexB;
            do
            {
                int relative = sibling;
                while ((relative = nodes[relative].relative) != sibling)
                {
                    int posC = nodes[relative].position;
                    if (posC != posA)
                    {
                        if (!IsEdgeInSurface(sibling, relative))
                        {
                            borderNodeB = relative;
                            goto skipB;
                        }
                    }
                }
            } while ((sibling = nodes[sibling].sibling) != nodeIndexB);

            skipB:;

            if (IsEdgeInSurface(nodeIndexA, nodeIndexB))
            {
                if (borderNodeA != -1 && borderNodeB != -1)
                {
                    return EdgeType.AShape;
                }
                else if (borderNodeA != -1)
                {
                    return EdgeType.TShapeA;
                }
                else if (borderNodeB != -1)
                {
                    return EdgeType.TShapeB;
                }
                else
                {
                    return EdgeType.Manifold;
                }
            }
            else
            {
                if (borderNodeA == -1 || borderNodeB == -1)
                    return EdgeType.Unknown; // Should not happen

                Debug.Assert(borderNodeA != -1 && borderNodeB != -1, "A border can't be connected to a manifold edge");
                return EdgeType.Border;
            }
        }

        public bool IsManifold(int nodeIndex, out int otherNodeIndex)
        {
            int relative = nodeIndex;
            while ((relative = nodes[relative].relative) != nodeIndex)
            {
                if (!IsEdgeManifold(relative, nodeIndex))
                {
                    otherNodeIndex = relative;
                    return false;
                }
                    
            }

            otherNodeIndex = -1;
            return true;
        }

        public bool IsManifold(int nodeIndex)
        {
            int relative = nodeIndex;
            while ((relative = nodes[relative].relative) != nodeIndex)
            {
                if (!IsEdgeManifold(relative, nodeIndex))
                    return false;
            }

            return true;
        }

        public bool IsEdgeManifold(int nodeIndexA, int nodeIndexB)
        {
            int posB = nodes[nodeIndexB].position;

            int facesAttached = 0;

            int siblingOfA = nodeIndexA;
            do // Iterator over faces around A
            {
                int relativeOfA = siblingOfA;
                do // Circulate around face
                {
                    int posC = nodes[relativeOfA].position;
                    if (posC == posB)
                    {
                        facesAttached++;
                    }
                } while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA);

            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            return facesAttached == 2;
        }

        public void Compact()
        {
            Dictionary<int, int> oldToNewNodeIndex = new Dictionary<int, int>();
            int validNodesCount = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved)
                {
                    validNodesCount++;
                }
            }

            Node[] newNodes = new Node[validNodesCount];

            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved)
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

            positionToNode = null;

            GC.Collect(); // Collect 
        }
    }
}