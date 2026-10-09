using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CourierParityAudit
{
    private const string ControllerPath = "Assets/Shiftbound/MaterialsV2/CourierMotion.controller";

    [InitializeOnLoadMethod]
    private static void RegisterBatchExit()
    {
        if (Environment.GetCommandLineArgs().Contains("-shiftboundProductionInput"))
        {
            Application.logMessageReceived -= OnTestMessage;
            Application.logMessageReceived += OnTestMessage;
        }
    }

    public static void RunGoldenInput()
    {
        foreach (string scene in new[] { "SkylineRooftops", "GoldenRooftops" })
        {
            EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/" + scene + ".unity");
            var motor = UnityEngine.Object.FindFirstObjectByType<Shiftbound.PlayerMotor>();
            var camera = UnityEngine.Object.FindFirstObjectByType<Shiftbound.FollowCamera>();
            Debug.Log("SHIFTBOUND SCENE BASELINE: " + scene + " speed=" + motor.maxSpeed +
                " jump=" + motor.jumpHeight + " gravity=" + motor.gravity + " coyote=" + motor.coyoteTime +
                " buffer=" + motor.jumpBuffer + " cameraDistance=" + camera.distance);
            foreach (string roof in new[] { "First landing", "First alternate bridge", "Shift landing" })
            {
                var collider = GameObject.Find(roof).GetComponent<BoxCollider>();
                Debug.Log("SHIFTBOUND SCENE ROOF: " + scene + " " + roof +
                    "=" + Shiftbound.GameplayCompareRunner.PlatformBounds(collider));
            }
        }
        string[] args = Environment.GetCommandLineArgs();
        int sceneOption = Array.IndexOf(args, "-shiftboundEditorScene");
        if (sceneOption >= 0 && sceneOption + 1 < args.Length)
        {
            string chosen = args[sceneOption + 1];
            if (chosen != "GoldenRooftops" && chosen != "SkylineRooftops")
                throw new Exception("Unsupported comparison scene " + chosen);
            EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/" + chosen + ".unity");
        }
        Application.logMessageReceived += OnTestMessage;
        EditorApplication.EnterPlaymode();
    }

    private static void OnTestMessage(string message, string stack, LogType type)
    {
        if (!message.StartsWith("SHIFTBOUND FULL ROUTE PASSED:") &&
            !message.StartsWith("SHIFTBOUND FULL ROUTE FAILED:")) return;
        int code = message.StartsWith("SHIFTBOUND FULL ROUTE PASSED:") ? 0 : 1;
        EditorApplication.delayCall += () => EditorApplication.Exit(code);
    }

    [MenuItem("Shiftbound/Milestone 1/Repair Courier Clip References")]
    public static void Repair()
    {
        // Resolve real imported subassets, rather than relying on a cached numeric FBX ID.
        AssetDatabase.ImportAsset(InspectRiggedAssets.AnimationPath, ImportAssetOptions.ForceUpdate);
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(InspectRiggedAssets.AnimationPath)
            .OfType<AnimationClip>().Where(x => !x.name.StartsWith("__preview__", StringComparison.Ordinal)).ToArray();
        foreach (ChildAnimatorState entry in controller.layers[0].stateMachine.states)
        {
            string name;
            switch (entry.state.name)
            {
                case "Idle": name = "Idle_Loop"; break;
                case "Jog": name = "Jog_Fwd_Loop"; break;
                case "Sprint": name = "Sprint_Loop"; break;
                case "Jump": name = "Jump_Loop"; break;
                case "Land": name = "Jump_Land"; break;
                default: continue;
            }
            AnimationClip clip = clips.FirstOrDefault(x => x.name == name ||
                x.name.EndsWith("|" + name, StringComparison.Ordinal) ||
                x.name.EndsWith("/" + name, StringComparison.Ordinal));
            if (clip == null) throw new Exception("Missing required courier clip " + name);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out string guid, out long identifier);
            Debug.Log("SHIFTBOUND COURIER CLIP: " + entry.state.name + "=" + clip.name +
                " id=" + identifier + " guid=" + guid + " length=" + clip.length);
            entry.state.motion = clip;
            EditorUtility.SetDirty(entry.state);
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.ForceReserializeAssets(new[] { ControllerPath });
        Debug.Log("SHIFTBOUND COURIER DEPENDENCIES: " + string.Join(",", AssetDatabase.GetDependencies(ControllerPath)));
    }
}
