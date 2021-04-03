using System;
using System.Collections.Generic;
using System.IO;

namespace Nanomesh
{
    public class ImporterOBJ
    {
        private const char _CHAR_SLASH = '/';
        private const int _SIZE_INIT = 1024;

        public static SharedMesh Read(string file)
        {
            SharedMesh mesh;
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    mesh = Load(reader);
                }
            }
            return mesh;
        }

        public static SharedMesh Load(StreamReader reader)
        {
            SharedMesh mesh = new SharedMesh();

            string[] brokenString;
            int offset = -1;

            List<Vector3> positions = new List<Vector3>(_SIZE_INIT);
            List<Vector3F> normals = new List<Vector3F>(_SIZE_INIT);
            List<Vector2F> uvs = new List<Vector2F>(_SIZE_INIT);
            List<int> triangles = new List<int>(_SIZE_INIT * 3);

            Dictionary<ObjVertexData, int> vertexData = new Dictionary<ObjVertexData, int>();

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                brokenString = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (brokenString.Length == 0)
                {
                    continue;
                }

                switch (brokenString[0])
                {
                    case "f":
                        ObjVertexData[] datas = new ObjVertexData[brokenString.Length - 1];
                        for (int x = 1; x < brokenString.Length; x++)
                        {
                            string[] split = brokenString[x].Split(_CHAR_SLASH);

                            datas[x - 1].position = split[0].ToInt() + offset;

                            if (split.Length > 1 && !string.IsNullOrEmpty(split[1]))
                            {
                                datas[x - 1].uv = split[1].ToInt() + offset;
                            }

                            if (split.Length > 2)
                            {
                                datas[x - 1].normal = split[2].ToInt() + offset;
                            }

                            if (!vertexData.ContainsKey(datas[x - 1]))
                            {
                                vertexData.Add(datas[x - 1], vertexData.Count);
                            }
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

            mesh.triangles = triangles.ToArray();

            mesh.positions = new Vector3[vertexData.Count];

            foreach (KeyValuePair<ObjVertexData, int> pair in vertexData)
            {
                mesh.positions[pair.Value] = positions[pair.Key.position];
            }

            if (uvs.Count > 0)
            {
                if (normals.Count > 0)
                {
                    var attr = new MetaAttributeList<Vector3F, Vector2F>(vertexData.Count);
                    foreach (var pair in vertexData)
                    {
                        attr.Set(new MetaAttribute<Vector3F, Vector2F>(normals[pair.Key.normal], uvs[pair.Key.uv]), pair.Value);
                    }
                    mesh.attributes = attr;
                    mesh.attributeDefinitions = new[] { new AttributeDefinition(AttributeType.Normals), new AttributeDefinition(AttributeType.UVs) };
                }
                else
                {
                    var attr = new MetaAttributeList<Vector2F>(vertexData.Count);
                    foreach (var pair in vertexData)
                    {
                        attr.Set(new MetaAttribute<Vector2F>(uvs[pair.Key.uv]), pair.Value);
                    }
                    mesh.attributes = attr;
                    mesh.attributeDefinitions = new[] { new AttributeDefinition(AttributeType.UVs) };
                }
            }
            else
            {
                if (normals.Count > 0)
                {
                    var attr = new MetaAttributeList<Vector3F>(vertexData.Count);
                    foreach (var pair in vertexData)
                    {
                        attr.Set(new MetaAttribute<Vector3F>(normals[pair.Key.normal]), pair.Value);
                    }
                    mesh.attributes = attr;
                    mesh.attributeDefinitions = new[] { new AttributeDefinition(AttributeType.Normals) };
                }
                else
                {
                    // No attributes :o)
                    mesh.attributeDefinitions = new AttributeDefinition[0];
                }
            }

            return mesh;
        }

        private struct ObjVertexData : IEquatable<ObjVertexData>
        {
            public int position;
            public int normal;
            public int uv;

            public override int GetHashCode()
            {
                unchecked
                {
                    return position ^ (normal << 2) ^ (uv >> 2);
                }
            }

            public bool Equals(ObjVertexData other)
            {
                return position == other.position
                    && normal == other.normal
                    && uv == other.uv;
            }
        }
    }
}
