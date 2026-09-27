using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Shiftbound;

public static class CaptureSceneRender
{
    [MenuItem("Shiftbound/Capture Skyline Preview")]
    public static void Capture()
    {
        EditorSceneManager.OpenScene("Assets/Shiftbound/Scenes/SkylineRooftops.unity", OpenSceneMode.Single);
        Camera camera = Camera.main;
        var player = Object.FindFirstObjectByType<PlayerMotor>();
        var worlds = Object.FindFirstObjectByType<WorldSwitcher>();
        if (camera == null || player == null || worlds == null)
            throw new System.Exception("Preview scene components missing");
        foreach (Renderer item in worlds.presentRoot.GetComponentsInChildren<Renderer>(true))
            item.enabled = false;
        foreach (Renderer item in worlds.alteredRoot.GetComponentsInChildren<Renderer>(true))
            item.enabled = true;
        camera.transform.position = player.transform.position + new Vector3(0f, 3.3f, -7.6f);
        camera.transform.LookAt(player.transform.position + new Vector3(0f, 1.4f, 7f));
        camera.fieldOfView = 66f;
        var target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        RenderTexture.active = target;
        camera.Render();
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        image.Apply();
        string path = "C:/Users/Admin/Desktop/LINDI/Shiftbound-Game/ArtDirection/SkylineRooftops-editor-preview.png";
        File.WriteAllBytes(path, image.EncodeToPNG());
        Debug.Log("SHIFTBOUND EDITOR CAPTURE: " + path);
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(image);
    }
}
