using UnityEngine;
using UVector3 = UnityEngine.Vector3;
using UVector2 = UnityEngine.Vector2;
using NVector3 = Nanomesh.Vector3;
using NVector2F = Nanomesh.Vector2F;
using NVector3F = Nanomesh.Vector3F;

namespace Nanomesh.Unity
{
    public static class UnityConverter
    {
        public static SharedMesh ToSharedMesh(this Mesh mesh)
        {
            UVector3[] vertices = mesh.vertices;
            UVector3[] normals = mesh.normals;
            UVector2[] uvs = mesh.uv;

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
            sharedMesh.groups = new Group[mesh.subMeshCount];

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var submeshDesc = mesh.GetSubMesh(i);
                sharedMesh.groups[i] = new Group { firstIndex = submeshDesc.indexStart, indexCount = submeshDesc.indexCount };
                Debug.Log($"Submesh {i} from {submeshDesc.indexStart} to {submeshDesc.indexStart + submeshDesc.indexCount}");
            }

            return sharedMesh;
        }

        public static Mesh ToUnityMesh(this SharedMesh sharedMesh)
        {
            Mesh newMesh = new Mesh();
            sharedMesh.ToUnityMesh(newMesh);
            return newMesh;
        }

        public static void ToUnityMesh(this SharedMesh sharedMesh, Mesh mesh)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

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
            mesh.subMeshCount = sharedMesh.groups.Length;

            for (int i = 0; i < sharedMesh.groups.Length; i++)
            {
                mesh.SetSubMesh(i, new UnityEngine.Rendering.SubMeshDescriptor(sharedMesh.groups[i].firstIndex, sharedMesh.groups[i].indexCount));
            }
        }
    }
}