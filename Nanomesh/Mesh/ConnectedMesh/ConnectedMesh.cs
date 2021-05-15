using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh
{
    // Let's say F = 2V
    // Halfedge mesh is V * sizeof(vertex) + 3F * sizeof(Halfedge) + F * sizeof(Face) = 16 * 0.5F + 3F * 20 + 4F = 72F
    // Connected mesh is V * sizeof(Vector3) + 3F * sizeof(Node) + F * sizeof(Face) = 12 * 0.5F + 3F * 12 + 12F = 54F (without attributes)
    // Connected mesh no face is V * sizeof(Vector3) + 3F * sizeof(Node) = 12 * 0.5F + 3F * 12 = 42F (without attributes)
    public partial class ConnectedMesh
    {
        // Todo : make this private (can only be modified from the inside)
        public Vector3[] positions;
        public MetaAttributeList attributes;
        public Node[] nodes;
        public Group[] groups;
        public AttributeDefinition[] attributeDefinitions;

        public int[] PositionToNode => _positionToNode ?? (_positionToNode = GetPositionToNode());
        private int[] _positionToNode;

        internal int _faceCount;
        public int FaceCount => _faceCount;

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

                if (faceEdgeCount != 3)
                    throw new NotImplementedException();

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

                        if (attributes != null)
                        {
                            for (int i = 0; i < attributes.CountPerAttribute; i++)
                            {
                                if (attrAtB != -1 && !attributes.Equals(attrAtB, nodes[relativeOfA].attribute, i))
                                {
                                    edgeWeight += attributeDefinitions[i].weight;
                                }

                                if (attrAtA != -1 && !attributes.Equals(attrAtA, nodes[siblingOfA].attribute, i))
                                {
                                    edgeWeight += attributeDefinitions[i].weight;
                                }
                            }
                        }

                        attrAtB = nodes[relativeOfA].attribute;
                        attrAtA = nodes[siblingOfA].attribute;
                    }
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            if (facesAttached < 2) // Border !
            {
                edgeWeight += EdgeBorderPenalty;
            }

            return edgeWeight;
        }

        internal static double EdgeBorderPenalty = 355.1594;

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
            // Rebuild nodes array with only valid nodes
            {
                int validNodesCount = 0;
                for (int i = 0; i < nodes.Length; i++)
                    if (!nodes[i].IsRemoved)
                        validNodesCount++;

                Node[] newNodes = new Node[validNodesCount];
                int k = 0;
                Dictionary<int, int> oldToNewNodeIndex = new Dictionary<int, int>();
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (!nodes[i].IsRemoved)
                    {
                        newNodes[k] = nodes[i];
                        oldToNewNodeIndex.Add(i, k);
                        k++;
                    }
                }
                for (int i = 0; i < newNodes.Length; i++)
                {
                    newNodes[i].relative = oldToNewNodeIndex[newNodes[i].relative];
                    newNodes[i].sibling = oldToNewNodeIndex[newNodes[i].sibling];
                }
                nodes = newNodes;
            }

            // Remap positions
            {
                Dictionary<int, int> oldToNewPosIndex = new Dictionary<int, int>();
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (!oldToNewPosIndex.ContainsKey(nodes[i].position))
                        oldToNewPosIndex.Add(nodes[i].position, oldToNewPosIndex.Count);

                    nodes[i].position = oldToNewPosIndex[nodes[i].position];
                }
                Vector3[] newPositions = new Vector3[oldToNewPosIndex.Count];
                foreach (KeyValuePair<int, int> oldToNewPos in oldToNewPosIndex)
                {
                    newPositions[oldToNewPos.Value] = positions[oldToNewPos.Key];
                }
                positions = newPositions;
            }

            // Remap attributes
            if (attributes != null)
            {
                Dictionary<int, int> oldToNewAttrIndex = new Dictionary<int, int>();
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (!oldToNewAttrIndex.ContainsKey(nodes[i].attribute))
                        oldToNewAttrIndex.Add(nodes[i].attribute, oldToNewAttrIndex.Count);

                    nodes[i].attribute = oldToNewAttrIndex[nodes[i].attribute];
                }
                MetaAttributeList newAttributes = attributes.CreateNew(oldToNewAttrIndex.Count);
                foreach (KeyValuePair<int, int> oldToNewAttr in oldToNewAttrIndex)
                {
                    newAttributes[oldToNewAttr.Value] = attributes[oldToNewAttr.Key];
                }
                attributes = newAttributes;
            }

            _positionToNode = null; // Invalid now
        }

        public void MergePositions(double tolerance = 0.01)
        {
            Dictionary<Vector3, int> newPositions = new Dictionary<Vector3, int>(tolerance <= 0 ? null : new Vector3Comparer(tolerance));

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

        public void MergeAttributes()
        {
            Dictionary<IMetaAttribute, int> _uniqueAttributes = new Dictionary<IMetaAttribute, int>();

            for (int i = 0; i < nodes.Length; i++)
            {
                _uniqueAttributes.TryAdd(attributes[nodes[i].attribute], nodes[i].attribute);
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].attribute = _uniqueAttributes[attributes[nodes[i].attribute]];
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
            _faceCount--;
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

        public SharedMesh ToSharedMesh()
        {
            // Compating here is an issue if mesh is being decimated :/
            //Compact();

            SharedMesh mesh = new SharedMesh();

            List<int> triangles = new List<int>();
            HashSet<int> browsedNodes = new HashSet<int>();

            Group[] newGroups = new Group[groups?.Length ?? 0];
            mesh.groups = newGroups;
            mesh.attributeDefinitions = attributeDefinitions;

            int currentGroup = 0;
            int indicesInGroup = 0;

            Dictionary<(int, int), int> perVertexMap = new Dictionary<(int, int), int>();

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
                        (int position, int attribute) key = (nodes[relative].position, nodes[relative].attribute);
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
            mesh.positions = new Vector3[perVertexMap.Count];
            foreach (KeyValuePair<(int, int), int> mapping in perVertexMap)
            {
                mesh.positions[mapping.Value] = positions[mapping.Key.Item1];
            }

            // Attributes
            if (attributes != null && attributeDefinitions.Length > 0)
            {
                mesh.attributes = attributes.CreateNew(perVertexMap.Count);
                foreach (KeyValuePair<(int, int), int> mapping in perVertexMap)
                {
                    mesh.attributes[mapping.Value] = attributes[mapping.Key.Item2];
                }
            }

            mesh.triangles = triangles.ToArray();

            return mesh;
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