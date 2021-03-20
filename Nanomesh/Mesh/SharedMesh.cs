using System.Diagnostics;

namespace Nanomesh
{
    /// <summary>
    /// A shared mesh is a flattened approach of the triangle mesh.
    /// Is does not has connectivity information, but it is simple to create
    /// and is a rather lightweight mesh data structure.
    /// it may have any attributes, but each
    /// </summary>
    public class SharedMesh
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Group[] groups;
        public Attributes attributes;

        [Conditional("DEBUG")]
        public void CheckLengths()
        {
            if (attributes != null)
            {
                foreach (var pair in attributes)
                {
                    Debug.Assert(pair.Value.Length == vertices.Length, $"Attribute '{pair.Value}' must have as many elements as vertices");
                }
            }
        }
    }
}