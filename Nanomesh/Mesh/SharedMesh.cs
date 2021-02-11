namespace Nanomesh
{
    public class SharedMesh
    {
        public Vector3[] vertices;
        public Vector3F[] normals;
        public Vector2F[] uvs;
        public BoneWeight[] boneWeights;
        public int[] triangles;
        public Group[] groups;

        public bool CheckLengths()
        {
            if (uvs != null && uvs.Length > 0 && vertices.Length != uvs.Length)
                return false;

            if (normals != null && normals.Length > 0 && vertices.Length != normals.Length)
                return false;

            if (boneWeights != null && boneWeights.Length > 0 && vertices.Length != boneWeights.Length)
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