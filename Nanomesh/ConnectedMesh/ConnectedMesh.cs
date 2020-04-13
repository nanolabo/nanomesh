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
                if (browsedNodes.Contains(i) || nodes[i].position < 0)
                    continue;

                // Only works if all elements are triangles
                int nextNodeIndex = i;
                do
                {
                    browsedNodes.Add(nextNodeIndex);
                    triangles.Add(nodes[nextNodeIndex].position);
                } while ((nextNodeIndex = nodes[nextNodeIndex].relative) != i);
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
                k++;
            return k;
        }

        [Obsolete]
        private int[] GetRelatives(int nodeIndex)
        {
            // Make room
            int k = 0;
            int nextNodeIndex = nodeIndex;
            while ((nextNodeIndex = nodes[nextNodeIndex].relative) != nodeIndex)
                k++;

            // Fill
            int[] res = new int[k];
            k = 0;
            while ((nextNodeIndex = nodes[nextNodeIndex].relative) != nodeIndex)
                res[k++] = nextNodeIndex;

            return res;
        }

        public int GetSiblingsCount(int nodeIndex)
        {
            int k = 0;
            int sibling = nodeIndex;
            while ((sibling = nodes[sibling].sibling) != nodeIndex)
                k++;
            return k;
        }

        [Obsolete]
        public IEnumerable<int> GetSiblings(int nodeIndex)
        {
            int nextNodeIndex = nodeIndex;
            do
            {
                nextNodeIndex = nodes[nextNodeIndex].sibling;
                yield return nextNodeIndex;
            }
            while (nextNodeIndex != nodeIndex);
        }

        public void ReconnectSiblings(int nodeIndex)
        {
            int sibling = nodeIndex;
            int lastValid = nodes[nodeIndex].position < 0 ? -1 : sibling;
            int firstValid = -1;
            int position = -1;

            do
            {
                if (nodes[sibling].position < 0)
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
            int lastValid = nodes[nodeIndexA].position < 0 ? -1 : sibling;
            int firstValid = -1;
            int position = -1;

            do
            {
                if (nodes[sibling].position < 0)
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
                if (nodes[sibling].position < 0)
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

        public void CollapseEdge(int nodeIndexA, int nodeIndexB)
        {
            //if (!IsAnEdge(nodeIndexA, nodeIndexB))
            //    throw new Exception("Given nodes doesn't make an edge !");

            positions[nodes[nodeIndexA].position] = (positions[nodes[nodeIndexA].position] + positions[nodes[nodeIndexB].position]) / 2;

            int siblingOfA = nodeIndexA;
            do
            {
                bool isFaceTouched = false;
                int faceEdgeCount = 0;
                int nodeIndexC = -1;

                int relativeOfA = siblingOfA;
                do
                {
                    if (AreNodesSiblings(relativeOfA, nodeIndexB))
                    {
                        nodes[relativeOfA].position = -1;
                        nodes[siblingOfA].position = -1;
                        isFaceTouched = true;
                    }
                    else if (relativeOfA != siblingOfA)
                    {
                        nodeIndexC = relativeOfA;
                    }

                    faceEdgeCount++;
                } while ((relativeOfA = nodes[relativeOfA].relative) != siblingOfA);

                if (isFaceTouched && faceEdgeCount == 2)
                {
                    nodes[nodeIndexC].position = -1;
                    ReconnectSiblings(nodeIndexC);
                }
            } while ((siblingOfA = nodes[siblingOfA].sibling) != nodeIndexA);

            ReconnectSiblings(nodeIndexA, nodeIndexB);
        }
    }
}