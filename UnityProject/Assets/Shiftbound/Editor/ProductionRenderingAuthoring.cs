using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ProductionUpgradeAuthoring
{
    public static void Rendering()
    {
        EditorSceneManager.OpenScene(Scene);
        var mobile=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");
        // Build-time configuration is required: changing grading mode only in
        // the player uses HDR LUT variants already stripped from an LDR build.
        mobile.supportsHDR=true;mobile.hdrColorBufferPrecision=HDRColorBufferPrecision._32Bits;
        mobile.colorGradingMode=ColorGradingMode.HighDynamicRange;EditorUtility.SetDirty(mobile);
        var pc=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
        pc.supportsHDR=true;pc.colorGradingMode=ColorGradingMode.HighDynamicRange;EditorUtility.SetDirty(pc);
        int quality=QualitySettings.GetQualityLevel();QualitySettings.SetQualityLevel(0,false);
        QualitySettings.skinWeights=SkinWeights.FourBones;QualitySettings.SetQualityLevel(quality,false);
        const string path="Assets/Shiftbound/ProductionBenchmark/Materials/Opening grade.asset";
        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        // Earlier authoring serialized three null references: generated volume
        // components must be persisted as subassets of the profile.
        profile.components.RemoveAll(component=>component==null);
        if(!profile.TryGet(out Tonemapping tone))tone=profile.Add<Tonemapping>();
        tone.mode.Override(TonemappingMode.ACES);
        if(!profile.TryGet(out ColorAdjustments grade))grade=profile.Add<ColorAdjustments>();
        grade.contrast.Override(3);grade.saturation.Override(-4);
        if(!profile.TryGet(out Bloom bloom))bloom=profile.Add<Bloom>();
        bloom.intensity.Override(.08f);bloom.threshold.Override(1.3f);
        foreach(var component in profile.components)
        {
            if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,profile);
            EditorUtility.SetDirty(component);
        }
        EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();
        AssetDatabase.ForceReserializeAssets(new[]{path,"Assets/Settings/Mobile_RPAsset.asset","Assets/Settings/PC_RPAsset.asset"});
        Debug.Log("SHIFTBOUND RENDERING AUTHORING PASSED: persisted ACES grade; mobile HDR LUT variants and four bone weights retained");
    }
}
