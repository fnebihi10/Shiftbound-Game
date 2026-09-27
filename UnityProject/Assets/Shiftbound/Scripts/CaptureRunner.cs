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
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(output);
            Debug.Log("SHIFTBOUND CAPTURE: " + output);
            yield return new WaitForSeconds(1.5f);
            Application.Quit();
        }
    }
}
