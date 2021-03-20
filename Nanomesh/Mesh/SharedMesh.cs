using System.Diagnostics;

namespace Nanomesh
{
    public class SharedMesh
    {
        public Vector3[] vertices;
        public AttributeListBase attributes;
        public int[] triangles;
        public Group[] groups;

        [Conditional("DEBUG")]
        public void CheckLengths()
        {
            Debug.Assert(attributes.Length == vertices.Length, $"Mesh must have as many attributes as vertices");
        }
    }

    public struct Group
    {
        public int firstIndex;
        public int indexCount;
    }
}