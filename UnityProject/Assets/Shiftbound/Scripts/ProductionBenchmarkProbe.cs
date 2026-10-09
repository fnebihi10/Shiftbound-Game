using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Profiling;
using UnityEngine;

namespace Shiftbound
{
    // Explicitly opt-in diagnostics. No capture/readback is performed during profiling.
    public sealed class ProductionBenchmarkProbe : MonoBehaviour
    {
        private string destination;
        private bool profile;
        private readonly FrameTiming[] timing = new FrameTiming[1];
        private readonly List<string> samples = new List<string>();
        private ProfilerRecorder batches, draws, triangles, allocation;
        private string phase;
        private double lastTimestamp;
        private bool collect;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-shiftboundProfile");
            bool profiling = index >= 0;
            if (!profiling) index = Array.IndexOf(args, "-shiftboundBenchmarkCaptures");
            if (index < 0 || index + 1 >= args.Length) return;
            var probe = new GameObject("Opt-in production benchmark probe").AddComponent<ProductionBenchmarkProbe>();
            probe.destination = args[index + 1];
            probe.profile = profiling;
        }

        private IEnumerator Start()
        {
            Directory.CreateDirectory(destination);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = profile ? -1 : 60;
            yield return new WaitForSecondsRealtime(2f);
            PlayerMotor motor = FindFirstObjectByType<PlayerMotor>();
            FollowCamera follow = FindFirstObjectByType<FollowCamera>();
            WorldSwitcher worlds = FindFirstObjectByType<WorldSwitcher>();
            motor.enabled = false;
            follow.enabled = false;
            motor.Teleport(new Vector3(0f, 0.08f, 1f));
            motor.Step(1f / 60f, Vector2.zero, false, false);
            follow.SetInspectionOrbit(0f, 18f);
            if (profile)
            {
                batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 1);
                draws = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1);
                triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", 1);
                allocation = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
                samples.Add("phase,realtime,cpu_frame_ms,cpu_main_ms,cpu_render_ms,present_wait_ms,gpu_ms,batches,draws,triangles,gc_bytes");
            }
            // Same positions/orbits in both states. Includes panorama seam/rear and pitch limits.
            foreach (int world in new[] { 0, 1 })
            {
                if (worlds.IsAltered != (world == 1))
                {
                    while (!worlds.IsReady) yield return null;
                    if (worlds.TrySwitch() != WorldSwitcher.ShiftResult.Accepted)
                        throw new InvalidOperationException("Benchmark switch rejected on shared support.");
                }
                foreach (float yaw in profile ? new[] { 0f, 90f, 180f, 270f } :
                    new[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f })
                {
                    follow.SetInspectionOrbit(yaw, 18f);
                    phase = (world == 0 ? "present" : "overgrown") + "-yaw-" + yaw.ToString("0", CultureInfo.InvariantCulture);
                    yield return new WaitForSecondsRealtime(profile ? 1.5f : 0.7f);
                    if (profile)
                    {
                        collect = true;
                        yield return new WaitForSecondsRealtime(3f);
                        collect = false;
                    }
                    else Capture(phase);
                }
                if (!profile)
                {
                    foreach (float pitch in new[] { -15f, 58f })
                    {
                        follow.SetInspectionOrbit(0f, pitch);
                        yield return new WaitForSecondsRealtime(0.5f);
                        Capture((world == 0 ? "present" : "overgrown") + "-pitch-" + pitch);
                    }
                    motor.Teleport(new Vector3(0f, 0.08f, 10f));
                    motor.Step(1f / 60f, Vector2.zero, false, false);
                    follow.SetInspectionOrbit(0f, 18f);
                    yield return new WaitForSecondsRealtime(0.5f);
                    Capture((world == 0 ? "present" : "overgrown") + "-first-lesson");
                    motor.Teleport(new Vector3(0f, 0.08f, 1f));
                    motor.Step(1f / 60f, Vector2.zero, false, false);
                }
            }
            if (profile)
            {
                File.WriteAllLines(Path.Combine(destination, "frames.csv"), samples);
                File.WriteAllText(Path.Combine(destination, "hardware.txt"),
                    "Unity=" + Application.unityVersion + "\nCPU=" + SystemInfo.processorType +
                    "\nRAM_MB=" + SystemInfo.systemMemorySize + "\nGPU=" + SystemInfo.graphicsDeviceName +
                    "\nGPU_API=" + SystemInfo.graphicsDeviceType + "\nDriver=" + SystemInfo.graphicsDeviceVersion +
                    "\nOS=" + SystemInfo.operatingSystem + "\nResolution=" + Screen.width + "x" + Screen.height +
                    "\nQuality=" + QualitySettings.names[QualitySettings.GetQualityLevel()] +
                    "\nVSync=0\nFrameCap=unlimited\nDevelopment=" + Debug.isDebugBuild +
                    "\nBatchesRecorder=" + batches.Valid + "\nDrawsRecorder=" + draws.Valid +
                    "\nTrianglesRecorder=" + triangles.Valid + "\nAllocationRecorder=" + allocation.Valid +
                    "\nSamples=" + (samples.Count - 1) +
                    "\nProtocol=stationary start roof; four orbits per world; 1.5s settle then 3s sample; no readbacks\n");
                Debug.Log("SHIFTBOUND RENDER PROFILE COMPLETE: rows=" + (samples.Count - 1));
            }
            else Debug.Log("SHIFTBOUND BENCHMARK CAPTURES COMPLETE: " + destination);
            Application.Quit(0);
        }

        private void Update()
        {
            if (!profile) return;
            FrameTimingManager.CaptureFrameTimings();
            if (!collect || FrameTimingManager.GetLatestTimings(1, timing) == 0) return;
            FrameTiming frame = timing[0];
            if (frame.frameStartTimestamp <= lastTimestamp) return;
            lastTimestamp = frame.frameStartTimestamp;
            samples.Add(string.Join(",", phase, Number(Time.realtimeSinceStartupAsDouble), Number(frame.cpuFrameTime),
                Number(frame.cpuMainThreadFrameTime), Number(frame.cpuRenderThreadFrameTime),
                Number(frame.cpuMainThreadPresentWaitTime), Number(frame.gpuFrameTime),
                Count(batches), Count(draws), Count(triangles), Count(allocation)));
        }

        private static string Number(double value) => value.ToString("F5", CultureInfo.InvariantCulture);
        private static string Count(ProfilerRecorder recorder) => recorder.Valid ? recorder.LastValue.ToString(CultureInfo.InvariantCulture) : "unavailable";

        private void Capture(string name)
        {
            Camera camera = Camera.main;
            var render = new RenderTexture(Screen.width, Screen.height, 24);
            RenderTexture oldTarget = camera.targetTexture, oldActive = RenderTexture.active;
            var image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = render;
                RenderTexture.active = render;
                camera.Render();
                image.ReadPixels(new Rect(0, 0, render.width, render.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(destination, name + ".png"), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                Destroy(render); Destroy(image);
            }
        }

        private void OnDestroy()
        {
            batches.Dispose(); draws.Dispose(); triangles.Dispose(); allocation.Dispose();
        }
    }
}
