using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nanomesh
{
    public class SharedMesh
    {
        public Vector3[] vertices;
        public Dictionary<AttributeType, IAttributeList> attributes;
        public int[] triangles;
        public Group[] groups;

        [Conditional("DEBUG")]
        public void CheckLengths()
        {
            foreach (var pair in attributes)
            {
                Debug.Assert(pair.Value.Length == vertices.Length, $"Attribute '{pair.Value}' must have as many elements as vertices");
            }
        }
    }

    public struct Group
    {
        public int firstIndex;
        public int indexCount;
    }
}