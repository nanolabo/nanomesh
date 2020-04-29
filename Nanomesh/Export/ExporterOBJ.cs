using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Nanolabo
{
    public static class ExporterOBJ
    {
        internal readonly static char CharSlash = '/';

        public static string ToInvariantString(this double value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static void Save(this SharedMesh mesh, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Write(mesh, fs);
            }
        }

        public static void Write(SharedMesh mesh, Stream stream, bool saveGroups = true, char groupChar = 'g')
        {
            HashSet<int> uniqueSubmeshes = new HashSet<int>();

            using (StreamWriter outfile = new StreamWriter(stream))
            {
                // Writting vertexes in file and making Index correspondance table.
                foreach (Vector3 vertex in mesh.vertices)
                {
                    outfile.WriteLine("v " + vertex.x.ToInvariantString() + " " + vertex.y.ToInvariantString() + " " + vertex.z.ToInvariantString());
                }

                Dictionary<int, int> normalMap = new Dictionary<int, int>();
                Dictionary<int, int> uvMap = new Dictionary<int, int>();
                int v = 1; // Counting offset for later
                int vn = 1; // Counting offset for later
                foreach (Vector3 vertex in mesh.vertices)
                {
                    //if (vertex.normal != null)
                    //{
                    //    outfile.WriteLine("vn " + (vertex.normal.x).ToInvariantString() + " " + (vertex.normal.y).ToInvariantString() + " " + (vertex.normal.z).ToInvariantString());
                    //    normalMap.Add(v, vn);
                    //    vn++;
                    //}
                    v++;
                }

                //foreach (Vertex2 uv in mesh.uvs)
                //{
                //    outfile.WriteLine("vt " + (uv.x).ToInvariantString() + " " + (uv.y).ToInvariantString());
                //}

                //mesh.triangles = mesh.triangles.OrderBy(x => x.submesh).ToList();

                // Writting faces data (with index shifting)
                for (int i = 0; i < mesh.triangles.Length; i += 3)
                {
                    //if (saveGroups && uniqueSubmeshes.Add(triangle.submesh))
                    //    outfile.WriteLine(groupChar + " " + mesh.subMeshes[triangle.submesh]);

                    // In obj files, indexing starts at 1
                    int ind1 = mesh.triangles[i] + 1;
                    int ind2 = mesh.triangles[i + 1] + 1;
                    int ind3 = mesh.triangles[i + 2] + 1;

                    //if (triangle.hasUVs())
                    //{
                    //    outfile.WriteLine(
                    //   "f " + ind1 + "/" + (triangle.uvs[0] + 1).ToString() + ((mesh.vertices[ind1 - 1].normal != null) ? "/" + normalMap[ind1].ToString() : null) +
                    //    " " + ind2 + "/" + (triangle.uvs[1] + 1).ToString() + ((mesh.vertices[ind2 - 1].normal != null) ? "/" + normalMap[ind2].ToString() : null) +
                    //    " " + ind3 + "/" + (triangle.uvs[2] + 1).ToString() + ((mesh.vertices[ind3 - 1].normal != null) ? "/" + normalMap[ind3].ToString() : null));
                    //}
                    //else
                    //{
                        outfile.WriteLine(
                       "f " + ind1 /*+ ((mesh.vertices[ind1 - 1].normal != null) ? "//" + normalMap[ind1].ToString() : null)*/ +
                        " " + ind2 /*+ ((mesh.vertices[ind2 - 1].normal != null) ? "//" + normalMap[ind2].ToString() : null)*/ +
                        " " + ind3 /*+ ((mesh.vertices[ind3 - 1].normal != null) ? "//" + normalMap[ind3].ToString() : null)*/);
                    //}
                }
            }
        }
    }
}
