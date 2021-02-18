using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh
{
    public enum AttributeType
    {
        Normals,
        UVs,
        BoneWeights,
        Colors,
    }

    // Let's say F = 2V
    // Halfedge mesh is V * sizeof(vertex) + 3F * sizeof(Halfedge) + F * sizeof(Face) = 16 * 0.5F + 3F * 20 + 4F = 72F
    // Connected mesh is V * sizeof(Vector3) + 3F * sizeof(Node) + F * sizeof(Face) = 12 * 0.5F + 3F * 12 + 12F = 54F (without attributes)
    // Connected mesh no face is V * sizeof(Vector3) + 3F * sizeof(Node) = 12 * 0.5F + 3F * 12 = 42F (without attributes)

    public partial class ConnectedMesh
    {
        public Vector3[] positions;
        public Attributes attributes;
        public Node[] nodes;
        public Group[] groups;

        public int[] PositionToNode => _positionToNode ?? (_positionToNode = GetPositionToNode());
        private int[] _positionToNode;

        internal int _faceCount;
        public int FaceCount => _faceCount;

        public static ConnectedMesh Build(SharedMesh mesh, bool copy = false)
        {
            mesh.CheckLengths();

            ConnectedMesh connectedMesh = new ConnectedMesh
            {
                groups = mesh.groups
            };

            int[] triangles = mesh.triangles;

            connectedMesh.attributes = new Attributes();

            if (copy)
            {
                throw new NotImplementedException();
            }
            else
            {
                connectedMesh.positions = mesh.vertices;
                connectedMesh.attributes = mesh.attributes;
            }

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
                {
                    vertexToNodes.Add(A.position, new List<int>());
                }

                if (!vertexToNodes.ContainsKey(B.position))
                {
                    vertexToNodes.Add(B.position, new List<int>());
                }

                if (!vertexToNodes.ContainsKey(C.position))
                {
                    vertexToNodes.Add(C.position, new List<int>());
                }

                vertexToNodes[A.position].Add(nodesList.Count);
                vertexToNodes[B.position].Add(nodesList.Count + 1);
                vertexToNodes[C.position].Add(nodesList.Count + 2);

                nodesList.Add(A);
                nodesList.Add(B);
                nodesList.Add(C);

                connectedMesh._faceCount++;
            }

            connectedMesh.nodes = nodesList.ToArray();

            foreach (KeyValuePair<int, List<int>> pair in vertexToNodes)
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

            Group[] newGroups = new Group[groups?.Length ?? 0];
            mesh.groups = newGroups;

            int currentGroup = 0;
            int indicesInGroup = 0;

            var perVertexMap = new Dictionary<(int positionIndex, int attributeIndex), int>();

            for (int i = 0; i < nodes.Length; i++)
            {
                if (newGroups.Length > 0 && groups[currentGroup].firstIndex == i)
                {
                    if (currentGroup > 0)
                    {
                        newGroups[currentGroup - 1].indexCount = indicesInGroup;
                        newGroups[currentGroup].firstIndex = indicesInGroup + newGroups[currentGroup - 1].firstIndex;
                    }
                    indicesInGroup = 0;
                    if (currentGroup < groups.Length - 1)
                    {
                        currentGroup++;
                    }
                }

                if (nodes[i].IsRemoved)
                {
                    continue;
                }

                indicesInGroup++;

                if (browsedNodes.Contains(i))
                {
                    continue;
                }

                // Only works if all elements are triangles
                int relative = i;
                do
                {
                    if (browsedNodes.Add(relative) && !nodes[relative].IsRemoved)
                    {
                        var key = (nodes[relative].position, nodes[relative].attribute);
                        perVertexMap.TryAdd(key, perVertexMap.Count);
                        triangles.Add(perVertexMap[key]);
                    }
                } while ((relative = nodes[relative].relative) != i);
            }

            if (newGroups.Length > 0)
            {
                newGroups[currentGroup].indexCount = indicesInGroup;
            }

            // Positions
            mesh.vertices = new Vector3[perVertexMap.Count];
            foreach (var mapping in perVertexMap)
            {
                mesh.vertices[mapping.Value] = positions[mapping.Key.positionIndex];
            }

            // Attributes
            mesh.attributes = new Attributes();
            foreach (var pair in attributes)
            {
                mesh.attributes.Add(pair.Key, pair.Value.CreateNew(perVertexMap.Count));
                IAttributeList destAttributes = mesh.attributes[pair.Key];
                IAttributeList fromAttributes = attributes[pair.Key];
                foreach (var mapping in perVertexMap)
                {
                    destAttributes[mapping.Value] = fromAttributes[mapping.Key.attributeIndex];
                }
            }

            mesh.triangles = triangles.ToArray();

            return mesh;
        }

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
                {
                    positionToNode[nodes[i].position] = i;
                }
            }

            return positionToNode;
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
                {
                    continue;
                }

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
            {
                return -1; // All siblings were removed
            }

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
                {
                    continue;
                }

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
                {
                    continue;
                }

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
            {
                return -1; // All siblings were removed
            }

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
            do // Iterates over faces around A
            {
                bool isFaceTouched = false;
                int faceEdgeCount = 0;
                int nodeIndexC = -1;

                int relativeOfA = siblingOfA;
                do // Circulate in face
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

                    if (_positionToNode != null)
                    {
                        _positionToNode[posC] = validNodeAtC;
                    }

                    _faceCount--;
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            int validNodeAtA = ReconnectSiblings(nodeIndexA, nodeIndexB, posA);

            if (_positionToNode != null)
            {
                _positionToNode[posA] = validNodeAtA;
                _positionToNode[posB] = -1;
            }

            return validNodeAtA;
        }

        public double GetEdgeTopo(int nodeIndexA, int nodeIndexB)
        {
            int posB = nodes[nodeIndexB].position;

            int facesAttached = 0;

            int attrAtA = -1;
            int attrAtB = -1;

            double edgeWeight = 0;

            int siblingOfA = nodeIndexA;
            do
            {
                int relativeOfA = siblingOfA;
                while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA)
                {
                    int posC = nodes[relativeOfA].position;
                    if (posC == posB)
                    {
                        facesAttached++;

                        foreach (var attr in attributes)
                        {
                            if (attrAtB != -1 && attrAtB != nodes[relativeOfA].attribute)
                            {
                                if (!attr.Value[attrAtB].Equals(attr.Value[nodes[relativeOfA].attribute]))
                                {
                                    edgeWeight += attr.Value.Weight;
                                }
                            }

                            if (attrAtA != -1 && attrAtA != nodes[siblingOfA].attribute)
                            {
                                if (!attr.Value[attrAtA].Equals(attr.Value[nodes[siblingOfA].attribute]))
                                {
                                    edgeWeight += attr.Value.Weight;
                                }
                            }
                        }

                        attrAtB = nodes[relativeOfA].attribute;
                        attrAtA = nodes[siblingOfA].attribute;
                    }
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            if (facesAttached < 3)
            {
                edgeWeight += 100;
            }

            return edgeWeight;
        }

        // TODO : Make it work with any polygon (other than triangle)
        public Vector3 GetFaceNormal(int nodeIndex)
        {
            int posA = nodes[nodeIndex].position;
            int posB = nodes[nodes[nodeIndex].relative].position;
            int posC = nodes[nodes[nodes[nodeIndex].relative].relative].position;

            Vector3 normal = Vector3.Cross(
                positions[posB] - positions[posA],
                positions[posC] - positions[posA]);

            return normal.Normalized;
        }

        // TODO : Make it work with any polygon (other than triangle)
        public double GetFaceArea(int nodeIndex)
        {
            int posA = nodes[nodeIndex].position;
            int posB = nodes[nodes[nodeIndex].relative].position;
            int posC = nodes[nodes[nodes[nodeIndex].relative].relative].position;

            Vector3 normal = Vector3.Cross(
                positions[posB] - positions[posA],
                positions[posC] - positions[posA]);

            return 0.5 * normal.Length;
        }

        // Only works with triangles !
        public double GetAngleRadians(int nodeIndex)
        {
            int posA = nodes[nodeIndex].position;
            int posB = nodes[nodes[nodeIndex].relative].position;
            int posC = nodes[nodes[nodes[nodeIndex].relative].relative].position;

            return Vector3.AngleRadians(
                positions[posB] - positions[posA],
                positions[posC] - positions[posA]);
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

            /*
            Attribute[] newAttributes = new Attribute[validAttrCount];
            Dictionary<int, int> oldToNewAttrIndex = new Dictionary<int, int>();
            for (int i = 0; i < attributes.Length; i++)
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
            */

            nodes = newNodes;
            positions = newPositions;

            // Invalidate mapping
            _positionToNode = null;
            //_attributeToNode = null;
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
            foreach (KeyValuePair<Vector3, int> pair in newPositions)
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

            _positionToNode = null;

            // Dereference faces that no longer exist
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].IsRemoved)
                {
                    continue;
                }

                int lastPos = nodes[i].position;
                int relative = i;
                while ((relative = nodes[relative].relative) != i) // Circulate around face
                {
                    int currPos = nodes[relative].position;
                    if (lastPos == currPos)
                    {
                        RemoveFace(relative);
                        break;
                    }
                    lastPos = currPos;
                }
            }
        }

        public void RemoveFace(int nodeIndex)
        {
            int relative = nodeIndex;
            do
            {
                nodes[relative].MarkRemoved();
                ReconnectSiblings(relative);
            } while ((relative = nodes[relative].relative) != nodeIndex);
        }

        public void Scale(double factor)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = positions[i] * factor;
            }
        }

        public HashSet<Edge> GetAllEdges()
        {
            HashSet<Edge> edges = new HashSet<Edge>();
            for (int p = 0; p < PositionToNode.Length; p++)
            {
                int nodeIndex = PositionToNode[p];
                if (nodeIndex < 0)
                {
                    continue;
                }

                int sibling = nodeIndex;
                do
                {
                    int firstRelative = nodes[sibling].relative;
                    int secondRelative = nodes[firstRelative].relative;

                    Edge pair = new Edge(nodes[firstRelative].position, nodes[secondRelative].position);

                    edges.Add(pair);

                } while ((sibling = nodes[sibling].sibling) != nodeIndex);
            }

            return edges;
        }
    }

    public struct Edge : IEquatable<Edge>
    {
        public int posA;
        public int posB;

        public Edge(int posA, int posB)
        {
            this.posA = posA;
            this.posB = posB;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return posA + posB;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals((Edge)obj);
        }

        public bool Equals(Edge pc)
        {
            if (ReferenceEquals(this, pc))
            {
                return true;
            }
            else
            {
                return (posA == pc.posA && posB == pc.posB) || (posA == pc.posB && posB == pc.posA);
            }
        }

        public static bool operator ==(Edge x, Edge y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(Edge x, Edge y)
        {
            return !x.Equals(y);
        }

        public override string ToString()
        {
            return $"<A:{posA} B:{posB}>";
        }
    }
}
