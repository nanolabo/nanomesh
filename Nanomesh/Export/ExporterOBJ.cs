using System.Globalization;
using System.IO;
using System.Text;

namespace Nanomesh
{
    public static class ExporterOBJ
    {
        public static string ToInvariantString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static void SaveToFile(this SharedMesh mesh, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                SaveToStream(mesh, fs);
            }
        }

        public static void SaveToStream(this SharedMesh mesh, Stream stream, bool saveGroups = true, char groupChar = 'g')
        {
            using (StreamWriter outfile = new StreamWriter(stream, Encoding.UTF8, 256, true))
            {
                bool hasUvs = mesh.attributes.ContainsKey(AttributeType.UVs);
                bool hasNormals = mesh.attributes.ContainsKey(AttributeType.Normals);

                outfile.WriteLine("g Default");

                // Writting vertexes in file and making Index correspondance table.
                foreach (Vector3 vertex in mesh.vertices)
                {
                    outfile.WriteLine("v " + vertex.x.ToInvariantString() + " " + vertex.y.ToInvariantString() + " " + vertex.z.ToInvariantString());
                }

                if (hasNormals)
                    foreach (Vector3F normal in mesh.attributes[AttributeType.Normals])
                        outfile.WriteLine("vn " + normal.x.ToInvariantString() + " " + normal.y.ToInvariantString() + " " + normal.z.ToInvariantString());
                    }
                }

                if (hasUvs)
                    foreach (Vector2F uv in mesh.attributes[AttributeType.UVs])
                        outfile.WriteLine("vt " + uv.x.ToInvariantString() + " " + uv.y.ToInvariantString());
                    }
                }

                // Writting faces data (with index shifting)
                for (int i = 0; i < mesh.triangles.Length; i += 3)
                {
                    // In obj files, indexing starts at 1
                    int ind1 = mesh.triangles[i] + 1;
                    int ind2 = mesh.triangles[i + 1] + 1;
                    int ind3 = mesh.triangles[i + 2] + 1;

                    if (hasUvs && hasNormals)
                    {
                        outfile.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", ind1, ind2, ind3));
                    }
                    else if (hasUvs)
                    {
                        outfile.WriteLine(string.Format("f {0}/{0}/ {1}/{1} {2}/{2}", ind1, ind2, ind3));
                    }
                    else if (hasNormals)
                    {
                        outfile.WriteLine(string.Format("f {0}//{0} {1}//{1} {2}//{2}", ind1, ind2, ind3));
                    }
                    else
                    {
                        outfile.WriteLine(string.Format("f {0} {1} {2}", ind1, ind2, ind3));
                    }
                }
            }
        }
    }
}
