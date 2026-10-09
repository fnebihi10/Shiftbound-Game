using System;
using Shiftbound;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MilestoneDelivery
{
    private const string ScenePath = "Assets/Shiftbound/Scenes/GoldenRooftops.unity";

    [MenuItem("Shiftbound/Milestone 1/Clarify First Shift Lesson")]
    public static void ClarifyFirstShiftLesson()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var checkpoint = GameObject.Find("First checkpoint")?.GetComponent<StageTrigger>();
        var landing = GameObject.Find("Shift landing");
        if (checkpoint == null || landing == null)
            throw new Exception("Authored first Shift lesson is missing; refusing to alter scene.");
        checkpoint.shiftBridgeLessonExit = landing.transform;
        EditorUtility.SetDirty(checkpoint);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        SliceDelivery.ValidateScene();
        Debug.Log("SHIFTBOUND SHIFT LESSON GUIDANCE SAVED: first checkpoint to shared Shift landing.");
    }

    [MenuItem("Shiftbound/Milestone 1/Maintain Current Golden Scene")]
    public static void Maintain()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var motor = UnityEngine.Object.FindFirstObjectByType<PlayerMotor>();
        var worlds = UnityEngine.Object.FindFirstObjectByType<WorldSwitcher>();
        if (motor == null || worlds == null) throw new Exception("Golden core systems missing.");
        worlds.SynchronizeProbe();
        foreach (var trigger in UnityEngine.Object.FindObjectsByType<StageTrigger>(FindObjectsSortMode.None))
        {
            if (trigger.kind != StageTrigger.TriggerKind.Checkpoint) continue;
            trigger.hasCheckpointPosition = true;
            trigger.checkpointPosition = new Vector3(trigger.transform.position.x,
                motor.Controller != null ? motor.Controller.skinWidth : motor.GetComponent<CharacterController>().skinWidth,
                trigger.transform.position.z + 0.35f);
            trigger.checkpointFacingYaw = 0f;
            if (trigger.name == "First checkpoint") trigger.checkpointHint =
                "Blue rooftops are previews: Shift makes them solid.\nHold Jump for longer jumps.";
            else if (trigger.name == "Midair checkpoint") trigger.checkpointHint =
                "Choose either route. Shift makes blue rooftops solid.";
            else trigger.checkpointHint = "Combine a full jump with a midair Shift on the last bridge.";
            trigger.shiftBridgeLessonExit = trigger.name == "First checkpoint"
                ? GameObject.Find("Shift landing").transform : null;
            EditorUtility.SetDirty(trigger);
        }
        EditorUtility.SetDirty(worlds.playerProbe);
        // Keep the authored scene player. Refresh only the obsolete reusable prefab.
        GameObject copy = UnityEngine.Object.Instantiate(motor.gameObject);
        copy.name = "Runner";
        copy.transform.position = Vector3.zero;
        var copyMotor = copy.GetComponent<PlayerMotor>();
        copyMotor.input = copy.AddComponent<GameInput>();
        copyMotor.view = null;
        PrefabUtility.SaveAsPrefabAsset(copy, "Assets/Shiftbound/Prefabs/Runner.prefab");
        UnityEngine.Object.DestroyImmediate(copy);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        SliceDelivery.ValidateScene();
        Debug.Log("SHIFTBOUND MILESTONE MAINTENANCE PASSED: shared checkpoints, synchronized probe, current courier prefab. Authored route retained.");
    }
}
