using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

public static class ProductionUpgradeAudit
{
    public static void Inspect()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        var courier = UnityEngine.Object.FindFirstObjectByType<RiggedCourierAnimator>();
        Debug.Log("UPGRADE COURIER ROOT: " + courier.name + " scale=" + courier.transform.lossyScale);
        var animator = courier.GetComponent<Animator>();
        foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone == HumanBodyBones.LastBone) continue;
            Transform t = animator.GetBoneTransform(bone);
            if (t != null) Debug.Log("UPGRADE BONE: " + bone + " " + t.name + " local=" + courier.transform.InverseTransformPoint(t.position));
        }
        foreach (var r in courier.GetComponentsInChildren<Renderer>())
            Debug.Log("UPGRADE COURIER MESH: " + r.name + " " + r.bounds + " materials=" + string.Join(",",r.sharedMaterials.Select(m=>m.name)));
        foreach (var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            if (r.enabled && !r.transform.IsChildOf(courier.transform) && (r.GetComponent<Collider>() != null || r.name.Contains("facade") || r.name.Contains("block")))
                Debug.Log("UPGRADE WORLD: " + r.name + " center=" + r.bounds.center + " size=" + r.bounds.size);
        foreach (string path in new[]{"Assets/Shiftbound/ThirdParty/Quaternius/Animation/UAL1_Standard.fbx","Assets/Shiftbound/ThirdParty/Quaternius/Animation2/UAL2_Standard.fbx"})
            foreach (var c in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview")))
                Debug.Log("UPGRADE CLIP: " + c.name + " duration=" + c.length);
        Debug.Log("SHIFTBOUND UPGRADE AUDIT PASSED");
    }
    public static void InspectCity()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Constructed paving")||r.name.StartsWith("Middle distance")))
        {
            var mesh=r.GetComponent<MeshFilter>().sharedMesh;
            var vertices=mesh.vertices;var normals=mesh.normals;
            Debug.Log("UPGRADE SKIN: "+r.name+" enabled="+r.enabled+" bounds="+r.bounds+" materials="+string.Join(",",r.sharedMaterials.Select(m=>m.name))+" top normals="+string.Join(",",Enumerable.Range(0,vertices.Length).Where(i=>vertices[i].y>r.bounds.max.y-.005f).Take(8).Select(i=>normals[i].ToString())));
        }
        Debug.Log("SHIFTBOUND CITY AUDIT PASSED");
    }
}
