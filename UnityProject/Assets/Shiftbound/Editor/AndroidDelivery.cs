using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

public static class AndroidDelivery
{
    private const string Scene = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";
    public static void Audit()
    {
        foreach (AnimationClip clip in AssetDatabase.LoadAllAssetsAtPath("Assets/Shiftbound/ThirdParty/Quaternius/Animation/UAL1_Standard.fbx").OfType<AnimationClip>())
            Debug.Log("SHIFTBOUND CLIP: " + clip.name + " length=" + clip.length + " human=" + clip.isHumanMotion);
        Debug.Log("SHIFTBOUND TOOLCHAIN: Android=" + BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android) +
            " Unity=" + Application.unityVersion + " GPU=" + SystemInfo.graphicsDeviceName + " API=" + SystemInfo.graphicsDeviceVersion);
        Debug.Log("SHIFTBOUND AUDIT PASSED");
    }

    public static void AuditAnimation2()
    {
        const string path = "Assets/Shiftbound/ThirdParty/Quaternius/Animation2/UAL2_Standard.fbx";
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
        importer.SaveAndReimport();
        foreach (AnimationClip clip in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>())
            if (!clip.name.StartsWith("__preview__")) Debug.Log("SHIFTBOUND CLIP2: " + clip.name + " length=" + clip.length + " human=" + clip.isHumanMotion);
        Debug.Log("SHIFTBOUND ANIMATION2 AUDIT PASSED");
    }

    [MenuItem("Shiftbound/Android/Configure Phone QA")]
    public static void Configure()
    {
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.shiftboundproject.shiftbound");
        PlayerSettings.bundleVersion = "0.2.0";
        PlayerSettings.Android.bundleVersionCode = 2;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.optimizedFramePacing = true;
        PlayerSettings.Android.forceInternetPermission = false;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        // QA artifacts use Unity's debug signing. Release credentials must be
        // supplied outside the project, then cleared after the build.
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.Android.keystoreName = "";
        PlayerSettings.Android.keyaliasName = "";
        PlayerSettings.Android.keystorePass = "";
        PlayerSettings.Android.keyaliasPass = "";
        AssetDatabase.SaveAssets();
        Debug.Log("SHIFTBOUND ANDROID CONFIGURED: API36/min26 ARM64 IL2CPP GLES3 landscape frame-pacing");
    }

    public static void BuildApk() => Build(false);
    public static void BuildBundle() => Build(true);
    private static void Build(bool bundle)
    {
        Configure();
        SliceDelivery.ValidateScene();
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new Exception("Android Build Support module missing for " + Application.unityVersion);
        string dir = Environment.GetEnvironmentVariable("SHIFTBOUND_BUILD_DIR");
        if (string.IsNullOrEmpty(dir)) dir = Path.GetFullPath("Builds/AndroidCandidate");
        Directory.CreateDirectory(dir);
        EditorUserBuildSettings.buildAppBundle = bundle;
        bool release = Environment.GetEnvironmentVariable("SHIFTBOUND_RELEASE_SIGNING") == "1";
        CandidateProvenance.Manifest identity = CandidateProvenance.Begin("Android", release ? "external-release" : "debug-QA");
        string builtOutput = null;
        try
        {
            if (release)
            {
                PlayerSettings.Android.keystoreName = Required("SHIFTBOUND_KEYSTORE");
                PlayerSettings.Android.keyaliasName = Required("SHIFTBOUND_KEY_ALIAS");
                PlayerSettings.Android.keystorePass = Required("SHIFTBOUND_KEYSTORE_PASSWORD");
                PlayerSettings.Android.keyaliasPass = Required("SHIFTBOUND_KEY_PASSWORD");
                PlayerSettings.Android.useCustomKeystore = true;
            }
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Scene }, target = BuildTarget.Android,
                locationPathName = Path.Combine(dir, bundle ? "Shiftbound-QA.aab" : "Shiftbound-QA.apk"),
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("SHIFTBOUND ANDROID BUILD FAILED: " + report.summary.result);
            builtOutput = report.summary.outputPath;
        }
        finally
        {
            PlayerSettings.Android.keystorePass = "";
            PlayerSettings.Android.keyaliasPass = "";
            PlayerSettings.Android.keystoreName = "";
            PlayerSettings.Android.keyaliasName = "";
            PlayerSettings.Android.useCustomKeystore = false;
            AssetDatabase.SaveAssets();
        }
        CandidateProvenance.Finish(identity, builtOutput);
        Debug.Log("SHIFTBOUND ANDROID BUILD PASSED: " + builtOutput + " signing=" + (release ? "external-release" : "debug-QA"));
    }
    private static string Required(string key) => Environment.GetEnvironmentVariable(key) ?? throw new Exception("Missing release signing variable " + key);
}
