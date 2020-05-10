using UnityEngine;
using UVector3 = UnityEngine.Vector3;
using UVector2 = UnityEngine.Vector2;
using NVector3 = Nanolabo.Vector3;
using NVector2F = Nanolabo.Vector2F;
using NVector3F = Nanolabo.Vector3F;

namespace Nanolabo.Unity
{
    public static class UnityConverter
    {
        public static SharedMesh ToSharedMesh(this Mesh mesh)
        {
            UVector3[] vertices = mesh.vertices;
            UVector3[] normals = mesh.normals;
            UVector2[] uvs = mesh.uv;
            int[] triangles = mesh.triangles;

            SharedMesh sharedMesh = new SharedMesh();

            sharedMesh.vertices = new NVector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                sharedMesh.vertices[i] = new NVector3(vertices[i].x, vertices[i].y, vertices[i].z);

            sharedMesh.normals = new NVector3F[normals.Length];
            for (int i = 0; i < normals.Length; i++)
                sharedMesh.normals[i] = new NVector3F(normals[i].x, normals[i].y, normals[i].z);

            sharedMesh.uvs = new NVector2F[uvs.Length];
            for (int i = 0; i < uvs.Length; i++)
                sharedMesh.uvs[i] = new NVector2F(uvs[i].x, uvs[i].y);

            sharedMesh.triangles = mesh.triangles;

            return sharedMesh;
        }

        public static Mesh ToUnityMesh(this SharedMesh sharedMesh)
        {
            Mesh mesh = new Mesh();

            UVector3[] vertices = new UVector3[sharedMesh.vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new UVector3((float)sharedMesh.vertices[i].x, (float)sharedMesh.vertices[i].y, (float)sharedMesh.vertices[i].z);

            UVector3[] normals = new UVector3[sharedMesh.normals.Length];
            for (int i = 0; i < normals.Length; i++)
                normals[i] = new UVector3(sharedMesh.normals[i].x, sharedMesh.normals[i].y, sharedMesh.normals[i].z);

            UVector2[] uvs = new UVector2[sharedMesh.uvs.Length];
            for (int i = 0; i < uvs.Length; i++)
                uvs[i] = new UVector2(sharedMesh.uvs[i].x, sharedMesh.uvs[i].y);

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = sharedMesh.triangles;

            return mesh;
        }
    }
}