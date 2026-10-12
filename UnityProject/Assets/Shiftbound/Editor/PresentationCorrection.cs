using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Shiftbound;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PresentationCorrection
{
    // Restore visual references only. Never replace the owner's scene or its
    // gameplay components/transforms with a historical scene wholesale.
    public static void Apply()
    {
        var candidate = EditorSceneManager.OpenScene(UpgradeProduction.Scene);
        const string temporary = "Assets/Shiftbound/Scenes/PresentationReferenceInspection.unity";
        if (File.Exists(temporary)) throw new InvalidOperationException("Reference inspection scene already exists.");
        string reference = Path.GetFullPath(Path.Combine(Application.dataPath, "../../.validation/ProductionUpgrade/Upgrade20261011/Reference/Assets/Shiftbound/Scenes/GoldenRooftops.unity"));
        File.Copy(reference, temporary);
        AssetDatabase.ImportAsset(temporary, ImportAssetOptions.ForceSynchronousImport);
        var baseline = EditorSceneManager.OpenScene(temporary, OpenSceneMode.Additive);
        int restored = 0;
        try
        {
            var target = candidate.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .ToDictionary(PathKey, t => t);
            foreach (var root in baseline.GetRootGameObjects())
            foreach (var source in root.GetComponentsInChildren<Transform>(true))
            {
                if (root.name == "Runner" || !target.TryGetValue(PathKey(source), out var dest)) continue;
                var oldRenderer = source.GetComponent<Renderer>();
                var renderer = dest.GetComponent<Renderer>();
                if (oldRenderer == null || renderer == null) continue;
                renderer.enabled = oldRenderer.enabled;
                renderer.sharedMaterials = oldRenderer.sharedMaterials;
                renderer.shadowCastingMode = oldRenderer.shadowCastingMode;
                renderer.receiveShadows = oldRenderer.receiveShadows;
                var oldMesh = source.GetComponent<MeshFilter>();
                var mesh = dest.GetComponent<MeshFilter>();
                if (oldMesh != null && mesh != null) mesh.sharedMesh = oldMesh.sharedMesh;
                // Keep the known unsupported floating growth proxies removed.
                if (renderer.name == "Overgrowth silhouette") renderer.enabled = false;
                EditorUtility.SetDirty(renderer);
                if (mesh != null) EditorUtility.SetDirty(mesh);
                restored++;
            }
            var oldArt = baseline.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RooftopWorldArt>(true)).Single();
            var art = candidate.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RooftopWorldArt>(true)).Single();
            art.presentDeck = oldArt.presentDeck;
            art.overgrownDeck = oldArt.overgrownDeck;
            art.overgrownOnly = oldArt.overgrownOnly.Where(g => g != null && target.ContainsKey(PathKey(g.transform)))
                .Select(g => target[PathKey(g.transform)].gameObject).ToArray();
            EditorUtility.SetDirty(art);
            foreach (var t in target.Values)
            {
                if (t == null) continue;
                string n = t.name;
                bool newKit = n == "Production roof and facade kit" ||
                    n.StartsWith("Attached rooftop shrubs", StringComparison.Ordinal) ||
                    n.StartsWith("Rooftop shrub branches", StringComparison.Ordinal) ||
                    (n.StartsWith("Roof ", StringComparison.Ordinal) &&
                        (n.EndsWith(" coping", StringComparison.Ordinal) || n.EndsWith(" gutter", StringComparison.Ordinal)));
                if (newKit)
                {
                    if (t.GetComponentsInChildren<Collider>(true).Length != 0)
                        throw new InvalidOperationException("Refusing to remove a decorative kit containing gameplay collision: " + n);
                    UnityEngine.Object.DestroyImmediate(t.gameObject);
                }
            }
            var courier = candidate.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<RiggedCourierAnimator>(true)).Single();
            var controller = (AnimatorController)courier.GetComponent<Animator>().runtimeAnimatorController;
            var states = controller.layers[0].stateMachine.states;
            var gait = (BlendTree)states.Single(s => s.state.name == "Locomotion").state.motion;
            var children = gait.children;
            // The scaled-muscle jog distorted the authored gait. Restore the
            // complete source clip while retaining continuous speed blending.
            children[0].threshold = 1.5f;
            children[1].motion = states.Single(s => s.state.name == "Jog").state.motion;
            children[1].threshold = 3f;
            children[1].cycleOffset = 0f;
            children[2].threshold = 7f;
            children[2].cycleOffset = 0f;
            gait.children = children;
            EditorUtility.SetDirty(gait);
            EditorSceneManager.MarkSceneDirty(candidate);
            EditorSceneManager.SaveScene(candidate);
            AssetDatabase.SaveAssets();
        }
        finally
        {
            EditorSceneManager.CloseScene(baseline, true);
            AssetDatabase.DeleteAsset(temporary);
            SceneManager.SetActiveScene(candidate);
        }
        SliceDelivery.ValidateScene();
        Debug.Log("SHIFTBOUND PRESENTATION CORRECTION PASSED: restored visual references=" + restored + "; rejected kit removed from scene; original jog restored; gameplay transforms untouched.");
    }

    static string PathKey(Transform t) => t.parent == null ? t.name : PathKey(t.parent) + "/" + t.GetSiblingIndex() + ":" + t.name;
}
