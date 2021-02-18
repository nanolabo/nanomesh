using System.Diagnostics;

namespace Nanomesh
{
    public class SharedMesh
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Group[] groups;
        public Attributes attributes;

        [Conditional("DEBUG")]
        public void CheckLengths()
        {
            foreach (var pair in attributes)
            {
                Debug.Assert(pair.Value.Length == vertices.Length, $"Attribute '{pair.Value}' must have as many elements as vertices");
            }
        }
    }
}
