using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace Nanomesh.Unity
{
    public static class Tests
    {
        [MenuItem("CONTEXT/LODGroup/💩 Auto Generate LODs", priority = 0)]
        public static void GenerateLODs(MenuCommand command)
        {
            LODGroup lodGroup = (LODGroup)command.context;

            var lods = lodGroup.GetLODs();

            // Cleanup
            for (int i = 1; i < lods.Length; i++)
            {
                foreach (Renderer renderer in lods[i].renderers)
                {
                    UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                }
            }

            // Assign LOD0
            Renderer[] renderers = lodGroup.GetComponentsInChildren<Renderer>();
            lods[0].renderers = renderers;

            // Build LODs
            for (int i = 1; i < lods.Length; i++) 
            {
                List<Renderer> lodRenderers = new List<Renderer>();

                foreach (Renderer renderer in renderers)
                {
                    if (renderer is MeshRenderer meshRenderer)
                    {
                        MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                        if (meshFilter)
                        {
                            GameObject gameObject = new GameObject(renderer.gameObject.name + "_LOD" + i);
                            gameObject.transform.parent = renderer.transform;
                            gameObject.transform.localPosition = UnityEngine.Vector3.zero;
                            gameObject.transform.localRotation = UnityEngine.Quaternion.identity;
                            gameObject.transform.localScale = UnityEngine.Vector3.one;

                            MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
                            MeshFilter mf = gameObject.AddComponent<MeshFilter>();

                            mr.sharedMaterials = meshRenderer.sharedMaterials;
                            mf.sharedMesh = Decimate(meshFilter.sharedMesh, lods[i - 1].screenRelativeTransitionHeight);

                            lodRenderers.Add(mr);
                        }
                    }
                    else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                    {
                        GameObject gameObject = new GameObject(renderer.gameObject.name + "_LOD" + i);
                        gameObject.transform.parent = renderer.transform;
                        gameObject.transform.localPosition = UnityEngine.Vector3.zero;
                        gameObject.transform.localRotation = UnityEngine.Quaternion.identity;
                        gameObject.transform.localScale = UnityEngine.Vector3.one;

                        SkinnedMeshRenderer smr = gameObject.AddComponent<SkinnedMeshRenderer>();
                        smr.bones = skinnedMeshRenderer.bones;
                        smr.rootBone = skinnedMeshRenderer.rootBone;

                        smr.sharedMaterials = skinnedMeshRenderer.sharedMaterials;
                        smr.sharedMesh = Decimate(skinnedMeshRenderer.sharedMesh, lods[i - 1].screenRelativeTransitionHeight);

                        lodRenderers.Add(smr);
                    }
                }

                Debug.Log($"LOD{i} created with {lodRenderers.Count} renderers at {100f * lods[i - 1].screenRelativeTransitionHeight}% poly ratio");
                lods[i].renderers = lodRenderers.ToArray();
            }

            lodGroup.SetLODs(lods);
        }

        [MenuItem("Nanolabo/Decimate 50% Edit")]
        public static void Decimate50_Edit()
        {
            HashSet<Mesh> uniqueMeshes = new HashSet<Mesh>();

            MeshFilter[] meshFilters = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
            foreach (var meshFilter in meshFilters)
            {
                Mesh sharedMesh = meshFilter.sharedMesh;
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

        private static void Decimate(in Mesh inputMesh, Mesh outputMesh, float targetRatio = 0.5f)
        {
            SharedMesh smesh = UnityConverter.ToSharedMesh(inputMesh);
            ConnectedMesh cmesh = ConnectedMesh.Build(smesh);
            cmesh.MergePositions(0.001);
            //cmesh.Scale(1000);
            //NormalsModifier normalsModifier = new NormalsModifier();
            //normalsModifier.Run(cmesh, 45);
            DecimateModifier decimateModifier = new DecimateModifier();
            Profiling.Start("decimate");
            //decimateModifier.Verbosed += DecimateModifier_Verbosed;
            decimateModifier.DecimateToRatio(cmesh, targetRatio);
            //decimateModifier.DecimateToPolycount(cmesh, 15000);
            //decimateModifier.DecimateToError(cmesh, 0.05f);
            //cmesh.Compact();
            Debug.Log(Profiling.End("decimate"));
            //cmesh.Scale(0.001);
            smesh = cmesh.ToSharedMesh();
            UnityConverter.ToUnityMesh(smesh, outputMesh);
            outputMesh.RecalculateTangents();
        }

        private static Mesh Decimate(Mesh mesh, float targetRatio = 0.5f)
        {
            Mesh outputMesh = new Mesh();
            outputMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            outputMesh.bindposes = mesh.bindposes;
            Decimate(in mesh, outputMesh, targetRatio);
            return outputMesh;
        }
    }
}