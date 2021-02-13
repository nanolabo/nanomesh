using System;

namespace Nanomesh
{
    public static partial class PrimitiveUtils
    {
        public static SharedMesh CreateSphere(float radius = 1f, int nbLong = 24, int nbLat = 16)
        {
            SharedMesh mesh = mesh = new SharedMesh();

			#region Vertices
			Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
			float _pi = MathF.PI;
			float _2pi = _pi * 2f;

			vertices[0] = new Vector3(0, 1, 0) * radius;
			for (int lat = 0; lat < nbLat; lat++)
			{
				float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
				float sin1 = MathF.Sin(a1);
				float cos1 = MathF.Cos(a1);

				for (int lon = 0; lon <= nbLong; lon++)
				{
					float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
					float sin2 = MathF.Sin(a2);
					float cos2 = MathF.Cos(a2);

					vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
				}
			}
			vertices[vertices.Length - 1] = new Vector3(0, 1, 0) * -radius;
			#endregion

			#region Triangles
			int nbFaces = vertices.Length;
			int nbTriangles = nbFaces * 2;
			int nbIndexes = nbTriangles * 3;
			int[] triangles = new int[nbIndexes];

			//Top Cap
			int i = 0;
			for (int lon = 0; lon < nbLong; lon++)
			{
				triangles[i++] = lon + 2;
				triangles[i++] = lon + 1;
				triangles[i++] = 0;
			}

			//Middle
			for (int lat = 0; lat < nbLat - 1; lat++)
			{
				for (int lon = 0; lon < nbLong; lon++)
				{
					int current = lon + lat * (nbLong + 1) + 1;
					int next = current + nbLong + 1;

					triangles[i++] = current;
					triangles[i++] = current + 1;
					triangles[i++] = next + 1;

					triangles[i++] = current;
					triangles[i++] = next + 1;
					triangles[i++] = next;
				}
			}

			//Bottom Cap
			for (int lon = 0; lon < nbLong; lon++)
			{
				triangles[i++] = vertices.Length - 1;
				triangles[i++] = vertices.Length - (lon + 2) - 1;
				triangles[i++] = vertices.Length - (lon + 1) - 1;
			}
			#endregion

			mesh.vertices = vertices;
			mesh.triangles = triangles;

			return mesh;
        }
    }
}
