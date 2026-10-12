using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Shiftbound;

// Stores the existing runtime opening view for the Editor's stopped Game view.
// FollowCamera still owns all camera motion in Play Mode.
public static class GameplayOpeningPreview
{
    public static void AuthorAndBuildWindows()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/GoldenRooftops.unity");
        var follow = UnityEngine.Object.FindFirstObjectByType<FollowCamera>();
        if (follow == null || follow.target == null) throw new Exception("Missing gameplay camera/target.");
        Physics.SyncTransforms();
        follow.Recover(follow.target.eulerAngles.y);
        for (int i = follow.transform.childCount - 1; i >= 0; i--)
        {
            var child = follow.transform.GetChild(i);
            if (child.name == "Camera collision probe" && (child.gameObject.hideFlags & HideFlags.DontSave) != 0)
                UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
        EditorUtility.SetDirty(follow.transform);
        EditorSceneManager.MarkSceneDirty(follow.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        var output = Environment.GetEnvironmentVariable("SHIFTBOUND_PREVIEW_PATH");
        if (!string.IsNullOrEmpty(output)) Capture(follow.GetComponent<Camera>(), output);
        Debug.Log("GAMEPLAY OPENING PREVIEW AUTHORED: " + follow.transform.position + "; runtime camera parameters unchanged.");
        SliceDelivery.BuildWindows();
    }

    private static void Capture(Camera camera, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var target = RenderTexture.GetTemporary(1280, 720, 24, RenderTextureFormat.ARGB32);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }
}
