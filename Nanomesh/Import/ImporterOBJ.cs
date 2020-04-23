using System;
using System.Collections.Generic;
using System.IO;

namespace Nanolabo
{
    public class ImporterOBJ
    {
        internal readonly static char CharSlash = '/';

        public static SharedMesh Read(string file)
        {
            SharedMesh mesh;
            using (StreamReader reader = new StreamReader(file)) {
                mesh = Load(reader.BaseStream);
            }
            return mesh;
        }

        public static SharedMesh Load(Stream stream)
        {
            SharedMesh mesh = new SharedMesh();

            string[] brokenString;
            int offset = -1; // - mesh.vertices.Count - 1;

            string[] e1, e2, e3, e4;
            List<Vector3> normals = new List<Vector3>(1024);

            using (StreamReader sr = new StreamReader(stream))
            {
                int vcount = 0;
                int ncount = 0;
                int tcount = 0;

                string line = String.Empty;
                while ((line = sr.ReadLine()) != null)
                {

                    string currentText = line.Trim();
                    brokenString = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (brokenString.Length == 0)
                        continue;

                    switch (brokenString[0])
                    {

                        case "f":
                            tcount++;
                            break;

                        case "vn":
                            ncount++;
                            break;

                        case "v":
                            vcount++;
                            break;
                    }
                }

                mesh.triangles = new int[tcount * 3];
                mesh.vertices = new Vector3[vcount];

                vcount = 0;
                tcount = 0;

                stream.Position = 0;
                sr.DiscardBufferedData();
                while ((line = sr.ReadLine()) != null)
                {
                    string currentText = line.Trim();
                    brokenString = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (brokenString.Length == 0)
                        continue;

                    switch (brokenString[0])
                    {
                        case "usemtl":
                            break;

                        case "usemap":
                            break;

                        case "mtllib":
                            break;

                        case "vt":
                            //mesh.uvs.Add(new Vertex2(
                            //    brokenString[1].ToDouble(),
                            //    brokenString[2].ToDouble()));
                            break;

                        case "vn":
                            //normals.Add(new Vector3(
                            //    brokenString[1].ToDouble(),
                            //    brokenString[2].ToDouble(),
                            //    brokenString[3].ToDouble()));
                            break;

                        case "v":
                            mesh.vertices[vcount++] = new Vector3(brokenString[1].ToFloat(), brokenString[2].ToFloat(), brokenString[3].ToFloat());
                            break;

                        case "vc":
                            break;
                    }
                }

                Vector3 n1, n2, n3;
                ncount = normals.Count;

                stream.Position = 0;
                sr.DiscardBufferedData();

                while ((line = sr.ReadLine()) != null)
                {
                    string currentText = line.Trim();
                    brokenString = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (brokenString.Length == 0)
                        continue;

                    switch (brokenString[0])
                    {

                        case "o":
                        case "g":
                            //mesh.subMeshes.Add(brokenString[1]);
                            break;

                        case "f":

                            // Handles any poly
                            for (int x = 2; x < (brokenString.Length - 1); x++)
                            {

                                e1 = brokenString[1].Split(CharSlash);
                                e2 = brokenString[x].Split(CharSlash);
                                e3 = brokenString[x + 1].Split(CharSlash);

                                // Vertices
                                mesh.triangles[tcount++] = e1[0].ToInt() + offset;
                                mesh.triangles[tcount++] = e2[0].ToInt() + offset;
                                mesh.triangles[tcount++] = e3[0].ToInt() + offset;

                                // Submesh
                                //triangle.submesh = mesh.subMeshes.Count - 1;
                                //mesh.triangles.Add(triangle);

                                // UVs (if present in the file)
                                if (e1.Length > 1 && e2.Length > 1 && e3.Length > 1)
                                {
                                    if (!string.IsNullOrEmpty(e1[1]) && !string.IsNullOrEmpty(e2[1]) && !string.IsNullOrEmpty(e3[1]))
                                    {
                                        //triangle.uvs = new int[3];
                                        //triangle.uvs[0] = e1[1].ToPosInt(vcount) + offset;
                                        //triangle.uvs[1] = e2[1].ToPosInt(vcount) + offset;
                                        //triangle.uvs[2] = e3[1].ToPosInt(vcount) + offset;
                                    }
                                }
                                else
                                {
                                    // ev < 2, it can't contain normals. Skip to next line.
                                    continue;
                                }

                                // Normals (if present in the file)
                                if (e1.Length > 2 && e2.Length > 2 && e3.Length > 2
                                    && !string.IsNullOrEmpty(e1[2]) && !string.IsNullOrEmpty(e2[2]) && !string.IsNullOrEmpty(e3[2]))
                                {
                                    //n1 = mesh.vertices[triangle.vertices[0]].normal = normals[e1[2].ToPosInt(ncount) + offset];
                                    //n2 = mesh.vertices[triangle.vertices[1]].normal = normals[e2[2].ToPosInt(ncount) + offset];
                                    //n3 = mesh.vertices[triangle.vertices[2]].normal = normals[e3[2].ToPosInt(ncount) + offset];
                                    //Vector3 nt = n1 + n2 + n3;
                                    //nt.normalize();
                                    //triangle.normal = nt;
                                }
                            }
                            break;
                    }
                }
            }

            return mesh;
        }
    }
}