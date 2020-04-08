namespace Nanolabo
{
    public static partial class PrimitiveUtils
    {
        public static SharedMesh CreatePlane(int sizeX = 10, int sizeY = 10)
        {
            SharedMesh mesh = mesh = new SharedMesh();

            var vertices = new Vector3[(sizeX + 1) * (sizeY + 1)];
            for (int i = 0, y = 0; y <= sizeY; y++)
            {
                for (int x = 0; x <= sizeX; x++, i++)
                {
                    vertices[i] = new Vector3(x, y, 0);
                }
            }
            mesh.vertices = vertices;

            int[] triangles = new int[sizeX * sizeY * 6];
            for (int ti = 0, vi = 0, y = 0; y < sizeY; y++, vi++)
            {
                for (int x = 0; x < sizeX; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + sizeX + 1;
                    triangles[ti + 5] = vi + sizeX + 2;
                }
            }
            mesh.triangles = triangles;

            return mesh;
        }
    }
}
