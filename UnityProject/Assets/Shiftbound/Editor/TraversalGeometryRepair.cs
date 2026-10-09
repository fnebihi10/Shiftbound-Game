using System;
using Shiftbound;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TraversalGeometryRepair
{
    [MenuItem("Shiftbound/Milestone 1/Improve First Bridge Landing Margin")]
    public static void ImproveFirstBridge()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity", OpenSceneMode.Single);
        var bridge = GameObject.Find("First alternate bridge").GetComponent<BoxCollider>();
        Bounds bounds = GameplayCompareRunner.PlatformBounds(bridge);
        if (Mathf.Abs(bounds.min.z - 13f) > 0.01f ||
            (Mathf.Abs(bounds.max.z - 16f) > 0.01f && Mathf.Abs(bounds.max.z - 18f) > 0.01f))
            throw new InvalidOperationException("First bridge differs from measured baseline; refusing to alter it.");
        if (Mathf.Abs(bounds.max.z - 18f) < 0.01f) return;
        // Keep its leading edge and width. Extend the visible, solid Overgrown roof
        // toward the shared landing: 3.5m gap becomes 1.5m, while the direct bypass
        // remains 7.5m and still requires using the Shift bridge.
        Vector3 scale = bridge.transform.localScale;
        scale.z *= 5f / 3f;
        bridge.transform.localScale = scale;
        bridge.transform.position += Vector3.forward;
        foreach (string name in new[] { "First alternate bridge edge", "First alternate bridge front route light" })
        {
            var marker = GameObject.Find(name);
            if (marker != null && marker.transform.position.z > 15.75f)
                marker.transform.position += Vector3.forward * 2f;
        }
        var sideLight = GameObject.Find("First alternate bridge left route light");
        if (sideLight != null)
        {
            sideLight.transform.position += Vector3.forward;
            Vector3 lightScale = sideLight.transform.localScale;
            lightScale.z *= 5f / 3f;
            sideLight.transform.localScale = lightScale;
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        SliceDelivery.ValidateScene();
        Debug.Log("SHIFTBOUND FIRST BRIDGE MARGIN SAVED: Overgrown bridge z=13..18; shared landing starts z=19.5. Direct no-Shift lesson gap unchanged.");
    }
    [MenuItem("Shiftbound/Milestone 1/Apply Measured Shift Lesson Repair")]
    public static void Apply()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity", OpenSceneMode.Single);
        Transform first = GameObject.Find("Shift landing").transform;
        Transform middle = GameObject.Find("Midair landing").transform;
        if (Mathf.Abs(first.position.z - 21f) < 0.01f && Mathf.Abs(middle.position.z - 35.5f) < 0.01f)
        { Debug.Log("SHIFTBOUND GEOMETRY REPAIR already applied."); return; }
        if (Mathf.Abs(first.position.z - 18f) > 0.01f || Mathf.Abs(middle.position.z - 30f) > 0.01f)
            throw new InvalidOperationException("Authored route differs from measured baseline; refusing to move it.");
        // Two gaps become 7.5m. Preserve each downstream obstacle and its art as a group.
        // No blockers, new colliders, changed motor values or regeneration.
        string[] roots = { "Shared rooftop structure", "Present world geometry", "Overgrown world geometry",
            "City art V3", "Art V4 - rooftop and distant skyline", "Visual Benchmark V5 - opening roofs" };
        foreach (string name in roots)
        {
            var root = GameObject.Find(name);
            if (root == null) continue;
            foreach (Transform child in root.transform)
            {
                float z = child.position.z;
                if (z < 16.4f || z > 60f) continue;
                child.position += Vector3.forward * (z >= 28.4f ? 5.5f : 3f);
            }
        }
        foreach (StageTrigger trigger in UnityEngine.Object.FindObjectsByType<StageTrigger>(FindObjectsSortMode.None))
        {
            float z = trigger.transform.position.z;
            float offset = z >= 28.4f ? 5.5f : z >= 16.4f ? 3f : 0f;
            trigger.transform.position += Vector3.forward * offset;
            trigger.checkpointPosition += Vector3.forward * offset;
            EditorUtility.SetDirty(trigger);
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        SliceDelivery.ValidateScene();
        Debug.Log("SHIFTBOUND GEOMETRY REPAIR SAVED: first/midair bypass gaps 7.5m; connecting intermediate jumps 3.5m. Present left route retained as intentional alternative.");
    }
}
