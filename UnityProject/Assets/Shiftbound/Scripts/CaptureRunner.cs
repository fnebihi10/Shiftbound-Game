using System;
using System.Collections;
using UnityEngine;

namespace Shiftbound
{
    // Runs only when the built player receives -shiftboundCapture <png path>.
    public sealed class CaptureRunner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void MaybeRun()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-shiftboundCapture");
            if (index < 0 || index + 1 >= args.Length) return;
            var runner = new GameObject("Shiftbound visual capture").AddComponent<CaptureRunner>();
            runner.output = args[index + 1];
        }

        private string output;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.6f);
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundCaptureOvergrown") >= 0)
            {
                var worlds = FindFirstObjectByType<WorldSwitcher>();
                if (worlds != null && !worlds.IsAltered) worlds.TrySwitch();
            }
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();
            var camera = Camera.main;
            var target = new RenderTexture(1280, 720, 24);
            camera.targetTexture = target;
            RenderTexture.active = target;
            var courier = GameObject.Find("Courier - rigged hoodie");
            if (courier != null)
            {
                var skins = courier.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (skins.Length > 0)
                {
                    Bounds bounds = skins[0].bounds;
                    for (int i = 1; i < skins.Length; i++) bounds.Encapsulate(skins[i].bounds);
                    Debug.Log("SHIFTBOUND CAPTURE DIAGNOSTIC: courier bounds=" + bounds.size +
                        " camera=" + camera.transform.position + " player=" + courier.transform.position +
                        " fov=" + camera.fieldOfView + " localScale=" + courier.transform.localScale);
                }
            }
            camera.Render();
            var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            System.IO.File.WriteAllBytes(output, image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(target);
            Destroy(image);
            Debug.Log("SHIFTBOUND CAPTURE: " + output);
            yield return new WaitForSeconds(1.5f);
            Application.Quit();
        }
    }
}
