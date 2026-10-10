using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Shiftbound
{
    // Explicit diagnostic. Callback intervals are not Android presentation times.
    // No CSV formatting, readback, disk writes or per-frame list growth while collecting.
    [DefaultExecutionOrder(400)]
    public sealed class GameplayTelemetry : MonoBehaviour
    {
        struct Sample
        {
            public double elapsed, interval, cpu, main, render, gpu, input;
            public long draws, triangles, gc, memory;
            public int shift;
        }
        private readonly List<Sample> samples = new List<Sample>(90000);
        private readonly FrameTiming[] timing = new FrameTiming[1];
        private ProfilerRecorder draws, triangles, gc, memory;
        private double started, previous;
        private float duration;
        private string destination;
        private bool collecting, useTiming = true;
        private int shiftMarker;
        private WorldSwitcher worlds;
        readonly List<string> warnings=new List<string>();
        public bool Collecting => collecting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded-=Install;
            SceneManager.sceneLoaded+=Install;
        }
        private static void Install(Scene scene,LoadSceneMode mode)
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-shiftboundGameplayProfile");
            if (i < 0 || i + 1 >= args.Length) return;
            var probe = new GameObject("Gameplay telemetry").AddComponent<GameplayTelemetry>();
            probe.useTiming = Array.IndexOf(args, "-shiftboundNoTiming") < 0;
            probe.duration = 60f;
            int seconds = Array.IndexOf(args, "-shiftboundProfileSeconds");
            if (seconds >= 0 && seconds + 1 < args.Length && float.TryParse(args[seconds + 1], out float value)) probe.duration = value;
            int sync = Array.IndexOf(args, "-shiftboundVSync");
            QualitySettings.vSyncCount = sync >= 0 ? 1 : 0;
            if (Array.IndexOf(args, "-shiftboundNoPost") >= 0 && Camera.main != null)
                Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            bool repeating=Array.IndexOf(args,"-shiftboundRepeatRoute")>=0;
            if(repeating)probe.duration=0;
            probe.Begin(repeating?Path.Combine(args[i+1],"route-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")):args[i + 1]);
        }

        public void Begin(string path = null)
        {
            if (collecting) return;
            destination = path ?? Path.Combine(Application.persistentDataPath, "QA-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            samples.Clear(); started = previous = Time.realtimeSinceStartupAsDouble;
            draws = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1);
            triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", 1);
            gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            memory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1);
            worlds = FindFirstObjectByType<WorldSwitcher>();
            if (worlds != null) worlds.ShiftAttempted += MarkShift;
            collecting = true;
            Application.logMessageReceived+=RecordWarning;
            Debug.Log("SHIFTBOUND GAMEPLAY PROFILE START: " + destination);
        }
        void RecordWarning(string message,string trace,LogType type)
        {
            if(type==LogType.Warning||type==LogType.Error||message.Contains("GfxDeviceD3D11Base::PresentFrame"))
                warnings.Add((Time.realtimeSinceStartupAsDouble-started).ToString("F6",CultureInfo.InvariantCulture)+","+Time.frameCount+",\""+message.Replace("\"","\"\"").Replace("\n"," ").Replace("\r"," ")+"\"");
        }
        private void MarkShift(WorldSwitcher.ShiftResult result) { shiftMarker = (int)result + 1; }
        private void LateUpdate()
        {
            if (!collecting) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) { previous = now; return; }
            var sample = new Sample { elapsed = now - started, interval = (now - previous) * 1000, input = -1,
                draws = draws.Valid ? draws.LastValue : -1, triangles = triangles.Valid ? triangles.LastValue : -1,
                gc = gc.Valid ? gc.LastValue : -1, memory = memory.Valid ? memory.LastValue : -1, shift = shiftMarker };
            previous = now; shiftMarker = 0;
            if (useTiming)
            {
                FrameTimingManager.CaptureFrameTimings();
                if (FrameTimingManager.GetLatestTimings(1, timing) > 0)
                { sample.cpu = timing[0].cpuFrameTime; sample.main = timing[0].cpuMainThreadFrameTime;
                  sample.render = timing[0].cpuRenderThreadFrameTime; sample.gpu = timing[0].gpuFrameTime; }
            }
            if (GameFlow.Instance != null && GameFlow.Instance.player.InputSampleFrame == Time.frameCount)
                sample.input = GameFlow.Instance.player.LastInputToMotorMs;
            samples.Add(sample);
            if (samples.Count == samples.Capacity || (duration > 0 && now - started >= duration))
            { End(); if (duration > 0) Application.Quit(0); }
        }
        public void End()
        {
            if (!collecting) return;
            collecting = false;
            Application.logMessageReceived-=RecordWarning;
            if (worlds != null) worlds.ShiftAttempted -= MarkShift;
            draws.Dispose(); triangles.Dispose(); gc.Dispose(); memory.Dispose();
            Directory.CreateDirectory(destination);
            warnings.Insert(0,"elapsed_s,frame,message");File.WriteAllLines(Path.Combine(destination,"warnings.csv"),warnings);
            using (var csv = new StreamWriter(Path.Combine(destination, "gameplay.csv")))
            {
                csv.WriteLine("elapsed_s,callback_ms,cpu_ms,main_ms,render_ms,gpu_ms,draws,triangles,gc_bytes,memory_bytes,shift_result,input_event_to_motor_ms");
                foreach (Sample s in samples) csv.WriteLine(string.Join(",", new[] {
                    s.elapsed.ToString("F5", CultureInfo.InvariantCulture), s.interval.ToString("F4", CultureInfo.InvariantCulture),
                    s.cpu.ToString("F4", CultureInfo.InvariantCulture), s.main.ToString("F4", CultureInfo.InvariantCulture),
                    s.render.ToString("F4", CultureInfo.InvariantCulture), s.gpu.ToString("F4", CultureInfo.InvariantCulture),
                    s.draws.ToString(), s.triangles.ToString(), s.gc.ToString(), s.memory.ToString(), s.shift.ToString(),
                    s.input.ToString("F4", CultureInfo.InvariantCulture) }));
            }
            File.WriteAllText(Path.Combine(destination, "device.txt"),
                "Unity=" + Application.unityVersion + "\nDevice=" + SystemInfo.deviceModel + "\nOS=" + SystemInfo.operatingSystem +
                "\nCPU=" + SystemInfo.processorType + "\nGPU=" + SystemInfo.graphicsDeviceName + "\nDriver/API=" + SystemInfo.graphicsDeviceVersion +
                "\nSystemRAMMB=" + SystemInfo.systemMemorySize + "\nScreen=" + Screen.width + "x" + Screen.height +
                "\nRefresh=" + Screen.currentResolution.refreshRateRatio + "\nQuality=" + QualitySettings.names[QualitySettings.GetQualityLevel()] +
                "\nRenderScale=" + ((UniversalRenderPipelineAsset)QualitySettings.renderPipeline).renderScale +
                "\nVSync=" + QualitySettings.vSyncCount + "\nFrameCap=" + Application.targetFrameRate + "\nFrameTiming=" + useTiming +
                "\nIntervals=LateUpdate callback; NOT presentation deadlines. Zero timing counters unavailable.\nInput=software Input System timestamp to motor; NOT physical input-to-photon.\n");
            Debug.Log("SHIFTBOUND GAMEPLAY PROFILE COMPLETE: " + destination + " samples=" + samples.Count);
        }
        private void OnApplicationPause(bool pause) { if (pause) End(); }
        private void OnDestroy() { End(); }
    }
}
