using UnityEngine;
using UnityEditor;
using Nanolabo;

public static class Tests
{
    [MenuItem("Nanolabo/Decimate 50%")]
    public static void Decimate50()
    {
        Undo.RegisterFullObjectHierarchyUndo(Selection.activeGameObject, "Decimate 50%");
        //Undo.RecordObject(Selection.activeGameObject, "Decimate 50%");

        MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        SharedMesh sharedMesh = Nanolabo.Unity.UnityConverter.ToSharedMesh(mesh);
        ConnectedMesh connectedMesh = ConnectedMesh.Build(sharedMesh);

        connectedMesh.MergePositions(0.01);

        Profiling.Start("Decimating");
        DecimateModifier decimateModifier = new DecimateModifier();
        decimateModifier.DecimateToRatio(connectedMesh, 0.50f);
        Debug.Log(Profiling.End("Decimating"));

        //connectedMesh.Compact();

        sharedMesh = connectedMesh.ToSharedMesh();
        mesh = Nanolabo.Unity.UnityConverter.ToUnityMesh(sharedMesh);

        meshFilter.sharedMesh = mesh;

        Undo.FlushUndoRecordObjects();
    }
}
