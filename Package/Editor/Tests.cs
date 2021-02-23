using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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

            // This allows the function to decimate every mesh a single time for all its instances and all its LODs
            Dictionary<Mesh, DecimateModifier> decimationModifiers = new Dictionary<Mesh, DecimateModifier>();

            Mesh decimateGradualy(Mesh mesh, float ratio)
            {
                if (!decimationModifiers.ContainsKey(mesh))
                {
                    SharedMesh smesh = UnityConverter.ToSharedMesh(mesh);
                    ConnectedMesh cmesh = ConnectedMesh.Build(smesh);
                    cmesh.MergePositions(0.001);

                    var decimationModifier = new DecimateModifier();
                    decimationModifier.Initialize(cmesh);

                    decimationModifiers.Add(mesh, decimationModifier);
                }

                decimationModifiers[mesh].DecimateToRatio(ratio);

                Mesh outputMesh = new Mesh();
                // We can use the same format since we expect less indices after decimation
                outputMesh.indexFormat = mesh.indexFormat;
                outputMesh.bindposes = mesh.bindposes;

                var sharedMesh = decimationModifiers[mesh].Mesh.ToSharedMesh();
                UnityConverter.ToUnityMesh(sharedMesh, outputMesh);
                outputMesh.RecalculateTangents(); // Mandatory for some shaders

                return outputMesh;
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
                            mf.sharedMesh = decimateGradualy(meshFilter.sharedMesh, lods[i - 1].screenRelativeTransitionHeight);

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
                        smr.sharedMesh = decimateGradualy(skinnedMeshRenderer.sharedMesh, lods[i - 1].screenRelativeTransitionHeight);

                        lodRenderers.Add(smr);
                    }
                }

                Debug.Log($"LOD{i} created with {lodRenderers.Count} renderers at {100f * lods[i - 1].screenRelativeTransitionHeight}% poly ratio");
                lods[i].renderers = lodRenderers.ToArray();
            }

            lodGroup.SetLODs(lods);
        }
    }
}