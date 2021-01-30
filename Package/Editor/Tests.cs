using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Nanomesh.Unity
{
    public static class Tests
    {
        [MenuItem("Nanolabo/Decimate 50% Edit")]
        public static void Decimate50_Edit()
        {
            HashSet<Mesh> uniqueMeshes = new HashSet<Mesh>();

            MeshFilter[] meshFilter = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var renderer in meshFilter)
            {
                Mesh sharedMesh = renderer.sharedMesh;
                if (sharedMesh != null)
                    uniqueMeshes.Add(sharedMesh);
            }

            SkinnedMeshRenderer[] skmRenderers = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skmRenderer in skmRenderers)
            {
                Mesh sharedMesh = skmRenderer.sharedMesh;
                if (sharedMesh != null)
                    uniqueMeshes.Add(sharedMesh);
            }

            foreach (var mesh in uniqueMeshes)
            {
                Decimate(mesh, mesh);
            }
        }

        [MenuItem("Nanolabo/Decimate 50% Create")]
        public static void Decimate50_Create()
        {
            Dictionary<Mesh, Action<Mesh>> meshToAssign = new Dictionary<Mesh, Action<Mesh>>();

            MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                Mesh sharedMesh = meshFilter.sharedMesh;
                if (sharedMesh != null)
                {
                    var assign = meshToAssign.GetOrAdd(sharedMesh, (m) => { });
                    assign += (m) => meshFilter.sharedMesh = m;
                    meshToAssign[sharedMesh] = assign;
                }
            }

            SkinnedMeshRenderer[] skmRenderers = Selection.activeGameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var skmRenderer in skmRenderers)
            {
                Mesh sharedMesh = skmRenderer.sharedMesh;
                if (sharedMesh != null)
                {
                    var assign = meshToAssign.GetOrAdd(sharedMesh, (m) => { });
                    assign += (m) => skmRenderer.sharedMesh = m;
                    meshToAssign[sharedMesh] = assign;
                }
            }

            foreach (var pair in meshToAssign)
            {
                Mesh newMesh = Decimate(pair.Key);
                pair.Value(newMesh);
            }
        }

        private static void Decimate(in Mesh inputMesh, Mesh outputMesh)
        {
            SharedMesh smesh = UnityConverter.ToSharedMesh(inputMesh);
            ConnectedMesh cmesh = ConnectedMesh.Build(smesh);
            cmesh.MergePositions(0.001);
            //NormalsModifier normalsModifier = new NormalsModifier();
            //normalsModifier.Run(cmesh, 45);
            DecimateModifier decimateModifier = new DecimateModifier();
            Profiling.Start("decimate");
            decimateModifier.DecimateToRatio(cmesh, 0.25f);
            //cmesh.Compact();
            Debug.Log(Profiling.End("decimate"));
            smesh = cmesh.ToSharedMesh();
            UnityConverter.ToUnityMesh(smesh, outputMesh);
        }

        private static Mesh Decimate(Mesh mesh)
        {
            Mesh outputMesh = new Mesh();
            outputMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Decimate(in mesh, outputMesh);
            return outputMesh;
        }
    }
}