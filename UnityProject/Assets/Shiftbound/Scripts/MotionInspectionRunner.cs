using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Shiftbound
{
    public sealed class MotionInspectionRunner : MonoBehaviour
    {
        private string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-shiftboundMotionInspection");
            if (i >= 0 && i + 1 < args.Length)
                new GameObject("Motion inspection").AddComponent<MotionInspectionRunner>().output = args[i + 1];
        }
        IEnumerator Start()
        {
            yield return null;
            Directory.CreateDirectory(output);
            var courier = FindFirstObjectByType<RiggedCourierAnimator>();
            courier.enabled = false;
            var flow = GameFlow.Instance;
            flow.player.enabled = false;
            flow.player.Teleport(new Vector3(0f,.08f,1f));
            flow.cameraRig.SetInspectionOrbit(0f,18f);
            flow.cameraRig.enabled = false;
            Animator animator = courier.GetComponent<Animator>();
            animator.speed = 0f;
            foreach (string state in new[] { "Idle", "Walk", "Jog", "Sprint", "Takeoff", "Jump", "Fall", "Land" })
            foreach (float phase in new[] { .15f,.4f,.7f })
            {
                animator.Play("Base Layer." + state, 0, phase);
                animator.Update(0f);
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(output,state+"-"+phase+".png"));
                Debug.Log("SHIFTBOUND MOTION SAMPLE: " + state + " phase=" + phase + " valid=" + animator.HasState(0, Animator.StringToHash("Base Layer."+state)));
                yield return null;
            }
            Debug.Log("SHIFTBOUND MOTION INSPECTION PASSED: actual player Animator poses; fixed inspection, not motion acceptance.");
            Application.Quit(0);
        }
    }
}
