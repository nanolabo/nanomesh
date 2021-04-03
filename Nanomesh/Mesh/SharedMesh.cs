using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh
{
    /// <summary>
    /// A shared mesh is a flattened approach of the triangle mesh.
    /// Is does not has connectivity information, but it is simple to create
    /// and is a rather lightweight mesh data structure.
    /// </summary>
    public class SharedMesh
    {
        public Vector3[] positions;
        public int[] triangles;
        public Group[] groups;
        public MetaAttributeList attributes;
        public AttributeDefinition[] attributeDefinitions;

        [Conditional("DEBUG")]
        public void CheckLengths()
        {
            //if (attributes != null)
            //{
            //    foreach (var pair in attributes)
            //    {
            //        Debug.Assert(pair.Value.Length == vertices.Length, $"Attribute '{pair.Value}' must have as many elements as vertices");
            //    }
            //}
        }

        public ConnectedMesh ToConnectedMesh()
        {
            CheckLengths();

            ConnectedMesh connectedMesh = new ConnectedMesh
            {
                groups = groups
            };

            connectedMesh.positions = positions;
            connectedMesh.attributes = attributes;
            connectedMesh.attributeDefinitions = attributeDefinitions;

            // Building relatives
            Node[] nodes = new Node[triangles.Length];
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

                A.relative = i + 1; // B
                B.relative = i + 2; // C
                C.relative = i; // A

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

                vertexToNodes[A.position].Add(i);
                vertexToNodes[B.position].Add(i + 1);
                vertexToNodes[C.position].Add(i + 2);

                nodes[i] = A;
                nodes[i + 1] = B;
                nodes[i + 2] = C;

                connectedMesh._faceCount++;
            }

            // Building siblings
            foreach (KeyValuePair<int, List<int>> pair in vertexToNodes)
            {
                int previousSibling = -1;
                int firstSibling = -1;
                foreach (int node in pair.Value)
                {
                    if (firstSibling != -1)
                    {
                        nodes[node].sibling = previousSibling;
                    }
                    else
                    {
                        firstSibling = node;
                    }
                    previousSibling = node;
                }
                nodes[firstSibling].sibling = previousSibling;
            }

            connectedMesh.nodes = nodes;

            Debug.Assert(connectedMesh.Check());

            return connectedMesh;
        }
    }
}