using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static class PhoneCandidateAuthoring
{
    const string Scene = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";
    const string Audio = "Assets/Shiftbound/AudioCandidate/";
    const string Library = "Assets/Shiftbound/ThirdParty/Quaternius/Animation/UAL1_Standard.fbx";
    public static void Apply()
    {
        EditorSceneManager.OpenScene(Scene);
        var courier = UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var animator = courier.GetComponent<Animator>();
        var controller = (AnimatorController)animator.runtimeAnimatorController;
        var machine = controller.layers[0].stateMachine;
        var clips = AssetDatabase.LoadAllAssetsAtPath(Library).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        // Extend the existing controller in place, preserving GUIDs and old states.
        Set(machine, clips, "Walk", "Walk_Loop");
        Set(machine, clips, "Takeoff", "Jump_Start");
        Set(machine, clips, "Fall", "Jump_Loop");
        var motion2 = AssetDatabase.LoadAllAssetsAtPath("Assets/Shiftbound/ThirdParty/Quaternius/Animation2/UAL2_Standard.fbx")
            .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        Set(machine, motion2, "Jump", "NinjaJump_Idle_Loop");
        // The tucked rise and extended descent must have different silhouettes.
        // The licensed UAL1 loop provides the extended descent; a purpose-made
        // courier fall remains an art dependency, rather than two identical states.
        Set(machine, clips, "Fall", "Jump_Loop");
        Set(machine, motion2, "Takeoff", "NinjaJump_Start");
        Set(machine, motion2, "Land", "NinjaJump_Land");
        EditorUtility.SetDirty(controller);
        var feedback = UnityEngine.Object.FindFirstObjectByType<FeedbackAudio>();
        feedback.footstepClip = Clip("footstep_concrete_000.ogg");
        feedback.footstepVariants = new[] { Clip("footstep_concrete_001.ogg"), Clip("footstep_concrete_002.ogg"), Clip("footstep_concrete_003.ogg") };
        feedback.jumpClip = Clip("footstep_concrete_003.ogg");
        feedback.landingClip = Clip("impactSoft_medium_000.ogg");
        feedback.hardLandingClip = Clip("impactSoft_heavy_000.ogg");
        feedback.shiftClip = Clip("highUp.ogg");
        feedback.blockedClip = Clip("highDown.ogg");
        feedback.checkpointClip = Clip("twoTone1.ogg");
        feedback.goalClip = Clip("threeTone1.ogg");
        feedback.presentAmbience = Clip("Present-Air.wav");
        feedback.overgrownAmbience = Clip("Overgrown-Rustle.wav");
        feedback.ambienceVolume = .32f;
        EditorUtility.SetDirty(feedback);
        foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { Audio.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            bool ambience = path.EndsWith(".wav", StringComparison.Ordinal);
            var settings = importer.defaultSampleSettings;
            settings.loadType = ambience ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = ambience ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM;
            settings.quality = .65f;
            importer.forceToMono = true;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        SliceDelivery.ValidateScene();
        Debug.Log("SHIFTBOUND PHONE AUTHORING PASSED: existing controller extended; saved source-backed cues and loops.");
    }
    static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + name) ?? throw new Exception("Missing audio source " + name);
    static void Set(AnimatorStateMachine machine, AnimationClip[] clips, string state, string suffix)
    {
        AnimationClip clip = clips.Single(c => c.name.EndsWith("|" + suffix, StringComparison.Ordinal));
        AnimatorState motion = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == state) ?? machine.AddState(state);
        motion.motion = clip; motion.writeDefaultValues = true;
        EditorUtility.SetDirty(motion);
    }

    public static void MotionPreview()
    {
        EditorSceneManager.OpenScene(Scene);
        var courier = UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        var follow = UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
        follow.SetInspectionOrbit(0f, 18f);
        Camera camera = Camera.main;
        string output = System.IO.Path.GetFullPath("../.validation/AndroidCandidate/MotionPoses");
        System.IO.Directory.CreateDirectory(output);
        var clips = AssetDatabase.LoadAllAssetsAtPath(Library).OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal));
        var rt = new RenderTexture(1280, 720, 24);
        camera.targetTexture = rt;
        AnimationMode.StartAnimationMode();
        try
        {
            foreach (AnimationClip clip in clips.Where(c => c.name.EndsWith("|Jump_Start") || c.name.EndsWith("|Jump_Loop")))
            foreach (float fraction in new[] { .15f, .35f, .55f, .75f })
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(courier.gameObject, clip, clip.length * fraction);
                AnimationMode.EndSampling();
                camera.Render(); RenderTexture.active = rt;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0,0,1280,720),0,0); image.Apply();
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(output, clip.name.Replace('|','-') + "-" + fraction + ".png"), image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
        }
        finally { AnimationMode.StopAnimationMode(); camera.targetTexture = null; RenderTexture.active = null; UnityEngine.Object.DestroyImmediate(rt); }
        Debug.Log("SHIFTBOUND MOTION PREVIEW PASSED: " + output);
    }
}
