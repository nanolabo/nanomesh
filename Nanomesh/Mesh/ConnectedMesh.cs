using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Nanolabo.ImporterOBJ;
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

        public int[] AttributeToNode => attributeToNode ?? (attributeToNode = GetAttributeToNode());
        private int[] attributeToNode;

        internal int faceCount;
        public int FaceCount => faceCount;

        public static ConnectedMesh Build(SharedMesh mesh)
        {
            Debug.Assert(mesh.CheckLengths(), "Attributes size mismatch");

            ConnectedMesh connectedMesh = new ConnectedMesh();

            int[] triangles = mesh.triangles;

            connectedMesh.positions = new Vector3[mesh.vertices.Length];
            connectedMesh.attributes = new Attribute[mesh.vertices.Length];

            for (int i = 0; i < mesh.vertices.Length; i++)
                connectedMesh.positions[i] = mesh.vertices[i];

            if (mesh.uvs != null)
                for (int i = 0; i < mesh.uvs.Length; i++)
                    connectedMesh.attributes[i].uv = mesh.uvs[i];

            if (mesh.normals != null)
                for (int i = 0; i < mesh.normals.Length; i++)
                    connectedMesh.attributes[i].normal = mesh.normals[i];

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

            var triangles = new List<int>();
            var browsedNodes = new HashSet<int>();
            var vertexData = new Dictionary<VertexData, int>();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (browsedNodes.Contains(i) || nodes[i].IsRemoved)
                    continue;

                // Only works if all elements are triangles
                int relative = i;
                do
                {
                    if (browsedNodes.Add(relative) && !nodes[relative].IsRemoved)
                    {
                        VertexData data = new VertexData();
                        data.position = nodes[relative].position;
                        data.uv = nodes[relative].attribute;
                        vertexData.TryAdd(data, vertexData.Count);

                        triangles.Add(vertexData[data]);
                    }
                } while ((relative = nodes[relative].relative) != i);
            }

            mesh.vertices = new Vector3[vertexData.Count];
            mesh.uvs = new Vector2F[vertexData.Count];
            mesh.normals = new Vector3F[vertexData.Count];

            foreach (var pair in vertexData)
            {
                mesh.vertices[pair.Value] = positions[pair.Key.position];
                mesh.uvs[pair.Value] = attributes[pair.Key.uv].uv;
                mesh.normals[pair.Value] = attributes[pair.Key.uv].normal;
            }

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

            for (int i = 0; i < positions.Length; i++)
            {
                positionToNode[i] = -1;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved)
                    positionToNode[nodes[i].position] = i;
            }
            return positionToNode;
        }

        public int[] GetAttributeToNode()
        {
            int[] attributeToNode = new int[attributes.Length];

            for (int i = 0; i < attributes.Length; i++)
            {
                attributeToNode[i] = -1;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved)
                    attributeToNode[nodes[i].attribute] = i;
            }
            return attributeToNode;
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

        public int CollapseEdge(int nodeIndexA, int nodeIndexB)
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

        public bool IsEdgeInSurface(int nodeIndexA, int nodeIndexB)
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

        public bool IsEdgeHard(int nodeIndexA, int nodeIndexB)
        {
            int posB = nodes[nodeIndexB].position;

            int attrAtA = -1;
            int attrAtB = -1;

            bool hardAtA = false;
            bool hardAtB = false;

            int siblingOfA = nodeIndexA;
            do // Iterator over faces around A
            {
                int relativeOfA = siblingOfA;
                while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA)
                {
                    int posC = nodes[relativeOfA].position;
                    if (posC == posB)
                    {
                        if (attrAtB != -1 && attrAtB != nodes[relativeOfA].attribute)
                            hardAtB = true;
                        attrAtB = nodes[relativeOfA].attribute;

                        if (attrAtA != -1 && attrAtA != nodes[siblingOfA].attribute)
                            hardAtA = true;
                        attrAtA = nodes[siblingOfA].attribute;
                    }
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            return hardAtA && hardAtB;
        }

        public IEdgeType GetEdgeType(int nodeIndexA, int nodeIndexB)
        {
            int borderNodeA = -1;
            int borderNodeB = -1;
            bool hardAtA = false;
            bool hardAtB = false;

            int posA = nodes[nodeIndexA].position;
            int posB = nodes[nodeIndexB].position;

            int attrIndex = -1; 
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
                if (nodes[sibling].attribute != attrIndex && attrIndex != -1)
                {
                    hardAtA = true;
                }
                attrIndex = nodes[sibling].attribute;
            } while ((sibling = nodes[sibling].sibling) != nodeIndexA);

            skipA:;

            attrIndex = -1;
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
                if (nodes[sibling].attribute != attrIndex && attrIndex != -1)
                {
                    hardAtB = true;
                }
                attrIndex = nodes[sibling].attribute;
            } while ((sibling = nodes[sibling].sibling) != nodeIndexB);

            skipB:;

            if (IsEdgeInSurface(nodeIndexA, nodeIndexB))
            {
                if ((borderNodeA != -1) && (borderNodeB != -1))
                {
                    return new IEdgeType.SURFACIC_BORDER_AB();
                }
                else if (borderNodeA != -1)
                {
                    if (hardAtB)
                        return new IEdgeType.SURFACIC_BORDER_A_HARD_B();
                    else
                        return new IEdgeType.SURFACIC_BORDER_A();
                }
                else if (borderNodeB != -1)
                {
                    if (hardAtA)
                        return new IEdgeType.SURFACIC_BORDER_B_HARD_A();
                    else
                        return new IEdgeType.SURFACIC_BORDER_B();
                }
                else
                {
                    if (hardAtB && hardAtB)
                    {
                        if (IsEdgeHard(nodeIndexA, nodeIndexB))
                            return new IEdgeType.SURFACIC_HARD_EDGE();
                        else
                            return new IEdgeType.SURFACIC_HARD_AB();
                    }
                    else if (hardAtA)
                        return new IEdgeType.SURFACIC_HARD_A();
                    else if (hardAtB)
                        return new IEdgeType.SURFACIC_HARD_B();
                    else
                        return new IEdgeType.SURFACIC();
                }
            }
            else
            {
                if (borderNodeA == -1 || borderNodeB == -1)
                    return new IEdgeType.UNKNOWN(); // Should not happen

                return new IEdgeType.BORDER_AB  { borderNodeA = borderNodeA, borderNodeB = borderNodeB };
            }
        }

        public void Compact()
        {
            int validNodesCount = 0;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved)
                {
                    validNodesCount++;
                }
            }

            int validPosCount = 0;
            for (int i = 0; i < positions.Length; i++)
            {
                if (PositionToNode[i] >= 0)
                {
                    validPosCount++;
                }
            }

            int validAttrCount = 0;
            for (int i = 0; i < attributes.Length; i++)
            {
                if (AttributeToNode[i] >= 0)
                {
                    validAttrCount++;
                }
            }

            Node[] newNodes = new Node[validNodesCount];
            Dictionary<int, int> oldToNewNodeIndex = new Dictionary<int, int>();
            for (int i = 0; i < nodes.Length; i++)
            {
                if (!nodes[i].IsRemoved)
                {
                    newNodes[oldToNewNodeIndex.Count] = nodes[i];
                    oldToNewNodeIndex.Add(i, oldToNewNodeIndex.Count);
                }
            }

            Vector3[] newPositions = new Vector3[validPosCount];
            Dictionary<int, int> oldToNewPosIndex = new Dictionary<int, int>();
            for (int i = 0; i < positions.Length; i++)
            {
                if (PositionToNode[i] >= 0)
                {
                    newPositions[oldToNewPosIndex.Count] = positions[i];
                    oldToNewPosIndex.Add(i, oldToNewPosIndex.Count);
                }
            }

            Attribute[] newAttributes = new Attribute[validAttrCount];
            Dictionary<int, int> oldToNewAttrIndex = new Dictionary<int, int>();
            for (int i = 0; i < positions.Length; i++)
            {
                if (AttributeToNode[i] >= 0)
                {
                    newAttributes[oldToNewAttrIndex.Count] = attributes[i];
                    oldToNewAttrIndex.Add(i, oldToNewAttrIndex.Count);
                }
            }

            for (int i = 0; i < newNodes.Length; i++)
            {
                newNodes[i].relative = oldToNewNodeIndex[newNodes[i].relative];
                newNodes[i].sibling = oldToNewNodeIndex[newNodes[i].sibling];
                newNodes[i].position = oldToNewPosIndex[newNodes[i].position];
                newNodes[i].attribute = oldToNewAttrIndex[newNodes[i].attribute];
            }

            nodes = newNodes;
            positions = newPositions;

            // Invalidate mapping
            positionToNode = null;
            attributeToNode = null;
        }

        public void MergePositions(double tolerance = 0.01)
        {
            Dictionary<Vector3, int> newPositions = new Dictionary<Vector3, int>(new Vector3Comparer(tolerance));

            for (int i = 0; i < positions.Length; i++)
            {
                newPositions.TryAdd(positions[i], newPositions.Count);
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].position = newPositions[positions[nodes[i].position]];
            }

            positions = new Vector3[newPositions.Count];
            foreach (var pair in newPositions)
            {
                positions[pair.Value] = pair.Key;
            }

            newPositions = null;

            // Remapping siblings
            Dictionary<int, int> posToLastSibling = new Dictionary<int, int>();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (posToLastSibling.ContainsKey(nodes[i].position))
                {
                    nodes[i].sibling = posToLastSibling[nodes[i].position];
                    posToLastSibling[nodes[i].position] = i;
                }
                else
                {
                    nodes[i].sibling = -1;
                    posToLastSibling.Add(nodes[i].position, i);
                }
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].sibling < 0)
                {
                    // Assign last sibling to close sibling loop
                    nodes[i].sibling = posToLastSibling[nodes[i].position];
                }
            }

            positionToNode = null;
        }

        public class Vector3Comparer : IEqualityComparer<Vector3>
        {
            private double tolerance;

            public Vector3Comparer(double tolerance)
            {
                this.tolerance = tolerance;
            }

            public bool Equals(Vector3 x, Vector3 y)
            {
                return (int)(x.x / tolerance) == (int)(y.x / tolerance)
                    && (int)(x.y / tolerance) == (int)(y.y / tolerance)
                    && (int)(x.z / tolerance) == (int)(y.z / tolerance);
            }

            public int GetHashCode(Vector3 obj)
            {
                return (int)(obj.x / tolerance) ^ ((int)(obj.y / tolerance) << 2) ^ ((int)(obj.z / tolerance) >> 2);
            }
        }
    }
}