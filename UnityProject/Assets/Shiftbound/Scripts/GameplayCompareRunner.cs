using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Shiftbound
{
    // Standalone diagnostic: -shiftboundCompareRun <capture.png>.
    // Drives the regular CharacterController motor through the first two jumps
    // and the first world-specific bridge. It does not replace human playtesting.
    public sealed class GameplayCompareRunner : MonoBehaviour
    {
        private string capturePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void MaybeRun()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-shiftboundCompareRun");
            if (index < 0 || index + 1 >= args.Length) return;
            GameplayCompareRunner runner = new GameObject("Gameplay comparison").AddComponent<GameplayCompareRunner>();
            runner.capturePath = args[index + 1];
        }

        private IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            PlayerMotor motor = FindFirstObjectByType<PlayerMotor>();
            WorldSwitcher worlds = FindFirstObjectByType<WorldSwitcher>();
            Camera camera = Camera.main;
            if (motor == null || worlds == null || camera == null)
            {
                Fail("Missing scene systems");
                yield break;
            }
            motor.enabled = false;
            for (int i = 0; i < 20; i++) yield return null;
            Time.captureFramerate = 60;

            int stage = 0;
            bool releaseNext = false;
            float peakHeight = motor.transform.position.y;
            float started = Time.realtimeSinceStartup;
            var intervalsMs = new List<float>(600);
            long previousTick = System.Diagnostics.Stopwatch.GetTimestamp();
            for (int frame = 0; frame < 900; frame++)
            {
                long tick = System.Diagnostics.Stopwatch.GetTimestamp();
                if (frame > 0) intervalsMs.Add((tick - previousTick) * 1000f /
                    System.Diagnostics.Stopwatch.Frequency);
                previousTick = tick;
                Vector3 position = motor.transform.position;
                bool press = false;
                bool release = releaseNext;
                releaseNext = false;
                if (position.y < -3f || Time.realtimeSinceStartup - started > 30f)
                {
                    Fail("Fell or timed out at stage " + stage + " position " + position);
                    yield break;
                }
                if (stage == 0 && position.z >= 5.6f && motor.IsGrounded)
                {
                    press = true;
                    stage = 1;
                }
                if (stage == 1 && position.z >= 10f && motor.IsGrounded)
                {
                    worlds.TrySwitch();
                    if (!worlds.IsAltered) { Fail("First bridge shift rejected"); yield break; }
                    stage = 2;
                }
                if (stage == 2 && position.z >= 11.1f && motor.IsGrounded)
                {
                    press = true;
                    releaseNext = true;
                    stage = 3;
                }
                if (stage == 3 && position.z >= 13.4f && motor.IsGrounded)
                {
                    if (!worlds.IsAltered || position.z > 16.2f)
                    { Fail("Missed the overgrown bridge"); yield break; }
                    stage = 4;
                }
                if (stage == 4 && position.z >= 15.1f && motor.IsGrounded)
                {
                    press = true;
                    releaseNext = true;
                    stage = 5;
                }
                if (stage == 5 && position.z >= 17f && motor.IsGrounded)
                {
                    Debug.Log("SHIFTBOUND COMPARE PATH PASS: " +
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name +
                        " first bridge reached; fixed-step frames=" + frame +
                        " peakY=" + peakHeight.ToString("F2") +
                        " host interval p50/p95/max ms=" +
                        Percentile(intervalsMs, 0.5f).ToString("F2") + "/" +
                        Percentile(intervalsMs, 0.95f).ToString("F2") + "/" +
                        Percentile(intervalsMs, 1f).ToString("F2") +
                        " resolution=" + Screen.width + "x" + Screen.height);
                    yield return new WaitForEndOfFrame();
                    Capture(camera);
                    Time.captureFramerate = 0;
                    Application.Quit(0);
                    yield break;
                }
                motor.Step(1f / 60f, Vector2.up, press, release);
                peakHeight = Mathf.Max(peakHeight, motor.transform.position.y);
                yield return null;
            }
            Fail("Did not reach first bridge exit; stage " + stage +
                " position " + motor.transform.position);
        }

        private static float Percentile(List<float> values, float fraction)
        {
            if (values.Count == 0) return 0f;
            values.Sort();
            return values[Mathf.Clamp(Mathf.CeilToInt(values.Count * fraction) - 1,
                0, values.Count - 1)];
        }

        private void Capture(Camera camera)
        {
            const int width = 1280;
            const int height = 720;
            var target = new RenderTexture(width, height, 24);
            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes(capturePath, image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(target);
            Destroy(image);
            Debug.Log("SHIFTBOUND COMPARE CAPTURE: " + capturePath);
        }

        private static void Fail(string reason)
        {
            Debug.LogError("SHIFTBOUND COMPARE PATH FAIL: " + reason);
            Time.captureFramerate = 0;
            Application.Quit(1);
        }
    }
}
