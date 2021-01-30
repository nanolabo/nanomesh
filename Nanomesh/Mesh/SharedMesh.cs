namespace Nanomesh
{
    public class SharedMesh
    {
        public Vector3[] vertices;
        public Vector3F[] normals;
        public Vector2F[] uvs;
        public int[] triangles;
        public Group[] groups;

        public void CheckValidity()
        {
            // Throw exceptions 
            for (int i = 0; i < triangles.Length; i+=3)
            {
                
            }
        }

        public bool CheckLengths()
        {
            if (uvs != null && uvs.Length > 0 && vertices.Length != uvs.Length)
                return false;

            if (normals != null && normals.Length > 0 && vertices.Length != normals.Length)
                return false;

            return true;
        }
    }

    public struct Group
    {
        public int firstIndex;
        public int indexCount;
    }
}