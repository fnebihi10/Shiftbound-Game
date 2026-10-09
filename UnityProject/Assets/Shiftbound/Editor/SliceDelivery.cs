using System;
using System.IO;
using Shiftbound;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class SliceDelivery
{
    private const string ScenePath = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";

    [MenuItem("Shiftbound/Polish Existing Golden Rooftops")]
    public static void PolishScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (StageTrigger trigger in UnityEngine.Object.FindObjectsByType<StageTrigger>(FindObjectsSortMode.None))
        {
            if (trigger.kind != StageTrigger.TriggerKind.Checkpoint || trigger.hasCheckpointPosition) continue;
            Vector3 position = trigger.transform.position;
            trigger.hasCheckpointPosition = true;
            trigger.checkpointPosition = new Vector3(position.x, 0.12f, position.z + 0.35f);
            EditorUtility.SetDirty(trigger);
        }
        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            EditorUtility.SetDirty(camera);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("SHIFTBOUND SCENE POLISH SAVED: " + ScenePath);
    }

    [MenuItem("Shiftbound/Build Windows Playable Slice")]
    public static void BuildWindows()
    {
        ValidateScene();
        string outputRoot = Environment.GetEnvironmentVariable("SHIFTBOUND_BUILD_DIR");
        if (string.IsNullOrEmpty(outputRoot))
            outputRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/WindowsPolished"));
        Directory.CreateDirectory(outputRoot);
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = Path.Combine(outputRoot, "Shiftbound.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("SHIFTBOUND BUILD FAILED: " + report.summary.result);
        Debug.Log("SHIFTBOUND BUILD PASSED: " + report.summary.outputPath);
    }

    [MenuItem("Shiftbound/Validate Golden Rooftops")]
    public static void ValidateScene()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        WorldSwitcher worlds = UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        PlayerMotor player = UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
        FollowCamera cameraRig = UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
        if (worlds == null || worlds.presentRoot == null || worlds.alteredRoot == null ||
            worlds.playerProbe == null || player == null || cameraRig == null)
            throw new Exception("Core scene references are missing.");
        GameFlow flow = UnityEngine.Object.FindFirstObjectByType<GameFlow>();
        if (flow == null || flow.player != player || flow.worlds != worlds || flow.input == null ||
            player.input == null || player.view == null || player.visual == null || cameraRig.input == null ||
            cameraRig.target != player.transform || worlds.input == null || worlds.feedback == null)
            throw new Exception("Required flow/input/camera/feedback references are missing or mismatched.");
        CharacterController shape = player.GetComponent<CharacterController>();
        if (Vector3.Distance(shape.transform.lossyScale, Vector3.one) > 0.001f ||
            worlds.playerProbe.GetComponentInParent<CharacterController>() != shape ||
            Vector3.Distance(worlds.playerProbe.transform.lossyScale, Vector3.one) > 0.001f)
            throw new Exception("Controller and Shift probe require unit scale and the same player parent.");
        worlds.SynchronizeProbe();
        Physics.SyncTransforms();
        RiggedCourierAnimator courier = UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        AnimatorController motion = courier != null ?
            courier.GetComponent<Animator>().runtimeAnimatorController as AnimatorController : null;
        if (motion == null || motion.layers.Length == 0)
            throw new Exception("Courier animation controller is missing.");
        bool uprightIdle = false;
        foreach (ChildAnimatorState entry in motion.layers[0].stateMachine.states)
        {
            if (entry.state.motion is AnimationClip runtimeClip &&
                runtimeClip.name.StartsWith("__preview__", StringComparison.Ordinal))
                throw new Exception("Courier state " + entry.state.name + " uses an Editor-only preview clip.");
            if (entry.state.name != "Idle" || entry.state.motion == null) continue;
            string clip = entry.state.motion.name;
            uprightIdle = clip == "Idle_Loop" ||
                clip.EndsWith("|Idle_Loop", StringComparison.Ordinal) ||
                clip.EndsWith("/Idle_Loop", StringComparison.Ordinal);
        }
        if (!uprightIdle)
            throw new Exception("Courier Idle state must use upright Idle_Loop, not Crouch_Idle_Loop.");
        int checkpoints = 0;
        int goals = 0;
        foreach (StageTrigger trigger in UnityEngine.Object.FindObjectsByType<StageTrigger>(FindObjectsSortMode.None))
        {
            if (trigger.kind == StageTrigger.TriggerKind.Goal) { goals++; continue; }
            checkpoints++;
            if (!trigger.hasCheckpointPosition)
                throw new Exception(trigger.name + " has no explicit safe spawn.");
            Vector3 anchor = trigger.checkpointPosition;
            if (!Physics.Raycast(anchor + Vector3.up * 0.5f, Vector3.down,
                out RaycastHit hit, 1.25f, player.groundMask, QueryTriggerInteraction.Ignore) ||
                hit.normal.y < Mathf.Cos(player.GetComponent<CharacterController>().slopeLimit * Mathf.Deg2Rad))
                throw new Exception(trigger.name + " spawn lacks a walkable shared floor.");
            if (hit.collider.transform.IsChildOf(worlds.presentRoot) || hit.collider.transform.IsChildOf(worlds.alteredRoot))
                throw new Exception(trigger.name + " must recover on shared support when preserving world state.");
            foreach (bool altered in new[] { false, true })
                if (!SpawnClear(shape, worlds, anchor, altered))
                    throw new Exception(trigger.name + " capsule is blocked in " + (altered ? "Overgrown" : "Present"));
        }
        if (checkpoints != 3 || goals != 1)
            throw new Exception("Expected three checkpoints and one goal.");
        foreach (Transform root in new[] { worlds.presentRoot, worlds.alteredRoot })
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        foreach (Material material in renderer.sharedMaterials)
            if (material == null) throw new Exception(renderer.name + " has an empty material slot.");
        Debug.Log("SHIFTBOUND SCENE VALIDATION PASSED: roots, probes, checkpoint anchors, walkable floors, goal, and material slots.");
    }

    private static bool SpawnClear(CharacterController shape, WorldSwitcher worlds, Vector3 position, bool altered)
    {
        // Include disabled world colliders explicitly; Physics overlaps alone cannot see them.
        foreach (Collider obstacle in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (obstacle.isTrigger || obstacle.transform.IsChildOf(shape.transform)) continue;
            bool present = obstacle.transform.IsChildOf(worlds.presentRoot);
            bool overgrown = obstacle.transform.IsChildOf(worlds.alteredRoot);
            if ((present && altered) || (overgrown && !altered)) continue;
            bool enabled = obstacle.enabled;
            try
            {
                obstacle.enabled = true;
                if (Physics.ComputePenetration(worlds.playerProbe, position, shape.transform.rotation,
                    obstacle, obstacle.transform.position, obstacle.transform.rotation, out _, out float depth) && depth > 0.035f)
                    return false;
            }
            finally { obstacle.enabled = enabled; }
        }
        return true;
    }
}
