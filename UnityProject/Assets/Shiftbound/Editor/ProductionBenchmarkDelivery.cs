using System.IO;
using UnityEditor;
using UnityEngine;

public static partial class ProductionBenchmarkDelivery
{
    public static void BuildBaseline()
    {
        PlayerSettings.enableFrameTimingStats = true;
        SliceDelivery.BuildWindows();
        Debug.Log("SHIFTBOUND BENCHMARK BASELINE BUILT: visual scene unchanged, frame timing enabled.");
    }

    public static void InspectAvailableAssets()
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(InspectRiggedAssets.AnimationPath))
            if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                Debug.Log("BENCHMARK AVAILABLE CLIP: " + clip.name + " seconds=" + clip.length);
    }
}
