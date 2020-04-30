using System;
using System.Collections.Generic;
using System.IO;

namespace Nanolabo
{
    public class ImporterOBJ
    {
        const char CHAR_SLASH = '/';
        const int SIZE_INIT = 1024;

        public static SharedMesh Read(string file)
        {
            SharedMesh mesh;
            using (StreamReader reader = new StreamReader(file)) {
                mesh = Load(reader);
            }
            return mesh;
        }

        public static SharedMesh Load(StreamReader reader)
        {
            SharedMesh mesh = new SharedMesh();

            string[] brokenString;
            int offset = -1;

            List<Vector3> positions = new List<Vector3>(SIZE_INIT);
            List<Vector3F> normals = new List<Vector3F>(SIZE_INIT);
            List<Vector2F> uvs = new List<Vector2F>(SIZE_INIT);
            List<int> triangles = new List<int>(SIZE_INIT * 3);

            Dictionary<VertexData, int> vertexData = new Dictionary<VertexData, int>();

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                brokenString = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (brokenString.Length == 0)
                    continue;

                switch (brokenString[0])
                {
                    case "f":
                        VertexData[] datas = new VertexData[brokenString.Length - 1];
                        for (int x = 1; x < brokenString.Length; x++)
                        {
                            var split = brokenString[x].Split(CHAR_SLASH);

                            datas[x - 1].position = split[0].ToInt() + offset;
                            if (split.Length > 1) datas[x - 1].uv = split[1].ToInt() + offset;
                            if (split.Length > 2) datas[x - 1].normal = split[2].ToInt() + offset;

                            if (!vertexData.ContainsKey(datas[x - 1]))
                                vertexData.Add(datas[x - 1], vertexData.Count);
                        }

                        // Handles any ngons
                        for (int x = 2; x < brokenString.Length - 1; x++)
                        {
                            triangles.Add(vertexData[datas[0]]);
                            triangles.Add(vertexData[datas[x - 1]]);
                            triangles.Add(vertexData[datas[x]]);
                        }
                        break;

                    case "v":
                        positions.Add(new Vector3(brokenString[1].ToDouble(), brokenString[2].ToDouble(), brokenString[3].ToDouble()));
                        break;

                    case "vt":
                        uvs.Add(new Vector2F(brokenString[1].ToFloat(), brokenString[2].ToFloat()));
                        break;

                    case "vn":
                        normals.Add(new Vector3F(brokenString[1].ToFloat(), brokenString[2].ToFloat(), brokenString[3].ToFloat()));
                        break;
                }
            }

            mesh.vertices = new Vector3[vertexData.Count];
            mesh.uvs = new Vector2F[vertexData.Count];
            mesh.normals = new Vector3F[vertexData.Count];

            foreach (var pair in vertexData)
            {
                mesh.vertices[pair.Value] = positions[pair.Key.position];

                if (uvs.Count > 0)
                    mesh.uvs[pair.Value] = uvs[pair.Key.uv];

                if (normals.Count > 0)
                    mesh.normals[pair.Value] = normals[pair.Key.normal];
            }

            mesh.triangles = triangles.ToArray();

            return mesh;
        }

        public struct VertexData : IEquatable<VertexData>
        {
            public int position;
            public int normal;
            public int uv;

            public override int GetHashCode()
            {
                unsafe
                {
                    return position ^ normal ^ uv;
                }
            }

            public bool Equals(VertexData other)
            {
                return position == other.position
                    && normal == other.normal
                    && uv == other.uv;
            }
        }
    }
}