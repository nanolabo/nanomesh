using UnityEngine;
using UnityEditor;
using Nanolabo;

public static class Tests
{
    [MenuItem("Nanolabo/Decimate 50%")]
    public static void Decimate50()
    {
        Debug.Log(DecimateModifier.Benchmark());
    }
}
