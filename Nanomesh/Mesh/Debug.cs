using System;
using System.Linq;

namespace Nanomesh
{
    public partial class ConnectedMesh
    {
        public string PrintSiblings(int nodeIndex)
        {
            int sibling = nodeIndex;
            string text = string.Join(" > ", Enumerable.Range(0, 12).Select(x => {
                string res = sibling.ToString() + (nodes[sibling].IsRemoved ? "(x)" : $"({nodes[sibling].position})");
                sibling = nodes[sibling].sibling;
                return res;
            }));
            return text + "...";
        }

        public string PrintRelatives(int nodeIndex)
        {
            int relative = nodeIndex;
            string text = string.Join(" > ", Enumerable.Range(0, 12).Select(x => {
                string res = relative.ToString() + (nodes[relative].IsRemoved ? "(x)" : $"({nodes[relative].position})");
                relative = nodes[relative].relative;
                return res;
            }));
            return text + "...";
        }

        public bool CheckRelatives(int nodeIndex)
        {
            if (nodes[nodeIndex].IsRemoved)
                throw new Exception($"Node {nodeIndex} is removed");

            int relative = nodeIndex;
            int edgecount = 0;
            int prevPos = -2;
            do
            {
                if (nodes[relative].position == prevPos)
                {
                    throw new Exception($"Two relatives or more share the same position : {PrintRelatives(nodeIndex)}");
                }

                if (edgecount > 50)
                {
                    throw new Exception($"Circularity relative violation : {PrintRelatives(nodeIndex)}");
                }

                if (nodes[relative].IsRemoved)
                {
                    throw new Exception($"Node {nodeIndex} is connected to the deleted relative {relative}");
                }

                prevPos = nodes[relative].position;
                edgecount++;

            } while ((relative = nodes[relative].relative) != nodeIndex);

            return true;
        }

        public bool CheckSiblings(int nodeIndex)
        {
            if (nodes[nodeIndex].IsRemoved)
                throw new Exception($"Node {nodeIndex} is removed");

            int sibling = nodeIndex;
            int cardinality = 0;
            do
            {
                if (cardinality > 1000)
                {
                    //throw new Exception($"Node {i}'s cardinality is superior to 50. It is likely to be that face siblings are not circularily linked");
                    throw new Exception($"Circularity sibling violation : {PrintSiblings(nodeIndex)}");
                }

                if (nodes[sibling].IsRemoved)
                {
                    throw new Exception($"Node {nodeIndex} has a deleted sibling {sibling}");
                }

                cardinality++;

            } while ((sibling = nodes[sibling].sibling) != nodeIndex);

            return true;
        }

        public bool Check()
        {
            for (int nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
            {
                if (nodes[nodeIndex].IsRemoved)
                    continue;

                CheckRelatives(nodeIndex);

                CheckSiblings(nodeIndex);

                if (GetEdgeCount(nodeIndex) == 2)
                    throw new Exception($"Node {nodeIndex} is part of a polygon of degree 2");
            }

            return true;
        }
    }
}