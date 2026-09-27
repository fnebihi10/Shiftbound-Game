using System.Linq;
using UnityEditor;
using UnityEngine;

public static class InspectRiggedAssets
{
    public const string CharacterPath = "Assets/Shiftbound/ThirdParty/Quaternius/Character/Casual_Humanoid.fbx";
    public const string AnimationPath = "Assets/Shiftbound/ThirdParty/Quaternius/Animation/UAL1_Standard.fbx";

    [MenuItem("Shiftbound/Inspect Rigged Assets")]
    public static void Inspect()
    {
        var characterImporter = AssetImporter.GetAtPath(CharacterPath) as ModelImporter;
        if (characterImporter == null) throw new System.Exception("Character FBX not found");
        characterImporter.animationType = ModelImporterAnimationType.Human;
        characterImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        characterImporter.SaveAndReimport();
        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(CharacterPath).OfType<Avatar>().FirstOrDefault();
        Debug.Log("SHIFTBOUND RIG: avatar=" + (avatar == null ? "null" : avatar.name) +
            " valid=" + (avatar != null && avatar.isValid) + " human=" + (avatar != null && avatar.isHuman));
        var character = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPath);
        foreach (var renderer in character.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            Debug.Log("SHIFTBOUND RIG: mesh=" + renderer.name + " materials=" +
                string.Join(",", renderer.sharedMaterials.Select(m => m == null ? "null" : m.name)));
        Debug.Log("SHIFTBOUND RIG: bounds=" +
            string.Join(",", character.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(r => r.bounds.ToString())));

        foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(CharacterPath).OfType<AnimationClip>())
            if (!clip.name.StartsWith("__preview__"))
                Debug.Log("SHIFTBOUND HOODIE CLIP: " + clip.name + " duration=" + clip.length);

        var animationImporter = AssetImporter.GetAtPath(AnimationPath) as ModelImporter;
        if (animationImporter == null) throw new System.Exception("Animation FBX not found");
        animationImporter.animationType = ModelImporterAnimationType.Human;
        animationImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        
        animationImporter.SaveAndReimport();
        foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(AnimationPath).OfType<AnimationClip>())
            if (!clip.name.StartsWith("__preview__"))
                Debug.Log("SHIFTBOUND CLIP: " + clip.name + " duration=" + clip.length);
    }
}



