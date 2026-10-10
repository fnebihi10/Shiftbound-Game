using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftbound
{
    // Full route using the production motor and actual checkpoint/goal callbacks.
    public sealed class GameplayCompareRunner : MonoBehaviour
    {
        private string capturePath;
        private int rate = 60;
        private string motionDirectory;
        private int motionFrame;
        private bool productionInput;
        private bool repeatRoute;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded-=OnSceneLoaded;
            SceneManager.sceneLoaded+=OnSceneLoaded;
        }
        private static void OnSceneLoaded(Scene scene,LoadSceneMode mode)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-shiftboundCompareRun");
            if (index < 0 || index + 1 >= args.Length) return;
            var runner = new GameObject("Full route regression").AddComponent<GameplayCompareRunner>();
            runner.capturePath = args[index + 1];
            runner.productionInput = Array.IndexOf(args, "-shiftboundProductionInput") >= 0;
            runner.repeatRoute = Array.IndexOf(args, "-shiftboundRepeatRoute") >= 0;
            int hz = Array.IndexOf(args, "-shiftboundStepRate");
            if (hz >= 0 && hz + 1 < args.Length && int.TryParse(args[hz + 1], out int value)) runner.rate = value;
            int motion = Array.IndexOf(args, "-shiftboundMotionFrames");
            if (motion >= 0 && motion + 1 < args.Length) runner.motionDirectory = args[motion + 1];
        }
        private float StepTime(int frame)
        {
            if (rate > 0) return 1f / rate;
            switch (frame % 4) { case 0: return 1f / 30f; case 1: return 1f / 120f;
                case 2: return 1f / 60f; default: return 1f / 144f; }
        }
        private IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            var motor = FindFirstObjectByType<PlayerMotor>();
            var worlds = FindFirstObjectByType<WorldSwitcher>();
            var flow = GameFlow.Instance;
            if (motor == null || worlds == null || flow == null || rate < 0 || rate > 1000)
            { Fail("Missing systems or invalid step rate"); yield break; }
            motor.enabled = productionInput;
            ProductionInputDriver driver = productionInput ? GetComponent<ProductionInputDriver>() : null;
            if(productionInput&&driver==null)driver=gameObject.AddComponent<ProductionInputDriver>();
            if (driver != null) driver.editorFrameRate = rate > 0 ? rate : 60;
            if (productionInput)
            {
                // Production movement and unscaled debounce must share real elapsed time.
                // Accelerated capture clocks can simulate movement during a real cooldown.
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = rate > 0 ? rate : 60;
            }
            if(driver!=null&&repeatRoute&&Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundSustainedCoverage")>=0)
            {
                // Actual production input, not teleportation: inspect foliage
                // and corresponding skyline in both worlds, then fall over the
                // opening parapet and exercise the real checkpoint recovery.
                foreach(bool altered in new[]{false,true})
                {
                    if(worlds.IsAltered!=altered)
                    {driver.Set(Vector2.zero,false,true);yield return new WaitForSecondsRealtime(.8f);driver.Set(Vector2.zero,false);}
                    driver.SetLook(new Vector2(.6f,0));yield return new WaitForSecondsRealtime(5.22f);
                    driver.SetLook(Vector2.zero);
                }
                driver.Set(Vector2.zero,false,true);yield return new WaitForSecondsRealtime(.8f);driver.Set(Vector2.zero,false);
                int recovery=flow.RecoverySequence;
                driver.Set(Vector2.left,true);yield return new WaitForSecondsRealtime(.65f);
                driver.Set(Vector2.left,false);
                float until=Time.realtimeSinceStartup+5;
                while(flow.RecoverySequence==recovery&&Time.realtimeSinceStartup<until)yield return null;
                driver.Set(Vector2.zero,false);yield return null;yield return null;
                if(flow.RecoverySequence==recovery||Vector3.Distance(motor.transform.position,flow.CheckpointPosition)>.25f)
                {Fail("Repeated workload failed actual fall/recovery coverage");yield break;}
                Debug.Log("SHIFTBOUND SUSTAINED COVERAGE PASSED: both-world full orbit; actual input-driven fall and checkpoint recovery");
            }
            if (motionDirectory != null)
            {
                Directory.CreateDirectory(motionDirectory);
                if (!productionInput)
                {
                    Time.captureFramerate = rate > 0 ? rate : 60;
                    Time.fixedDeltaTime = 1f / Time.captureFramerate;
                }
            }
            Transform oldView = motor.view;
            if (!productionInput) motor.view = null;
            string[] roofs = { "Start rooftop", "First landing", "First alternate bridge", "Shift landing",
                "Midair takeoff", "Midair destination", "Midair landing", "Right route", "Return route",
                "Route landing", "Final present", "Final alternate", "Goal rooftop" };
            int[] states = { -1, -1, 1, -1, 0, 1, -1, 1, 0, -1, 0, 1, -1 };
            int frame = 0;
            for (int leg = 1; leg < roofs.Length; leg++)
            {
                if (driver != null) { driver.Set(Vector2.zero, false); yield return null; }
                var source = GameObject.Find(roofs[leg - 1]).GetComponent<BoxCollider>();
                var target = GameObject.Find(roofs[leg]).GetComponent<BoxCollider>();
                Bounds a = PlatformBounds(source), b = PlatformBounds(target);
                Vector2 direction = new Vector2(b.center.x - a.center.x, b.center.z - a.center.z).normalized;
                float edge = Mathf.Min(Mathf.Abs(direction.x) < 0.001f ? float.PositiveInfinity : a.extents.x / Mathf.Abs(direction.x),
                    Mathf.Abs(direction.y) < 0.001f ? float.PositiveInfinity : a.extents.z / Mathf.Abs(direction.y));
                Vector2 takeoff = new Vector2(a.center.x, a.center.z) + direction * Mathf.Max(0f, edge - 0.55f);
                bool launched = false, arrived = false, shifted = false;
                for (int step = 0; step < 1500; step++, frame++)
                {
                    Vector3 p = motor.transform.position;
                    float dt = StepTime(frame);
                    Vector2 position = new Vector2(p.x, p.z);
                    Vector2 goal = launched ? new Vector2(b.center.x, b.center.z) : takeoff;
                    Vector2 axes = Vector2.ClampMagnitude((goal - position) / (launched ? 1.25f : 0.6f), 1f);
                    bool press = !launched && motor.IsGrounded && Vector2.Distance(position, takeoff) < 0.18f;
                    if (press)
                    {
                        launched = true; axes = direction;
                        if (productionInput) Debug.Log("SHIFTBOUND PRODUCTION TAKEOFF: " + roofs[leg] +
                            " position=" + p + " world=" + worlds.IsAltered + " frame=" + frame);
                    }
                    bool requestShift = false;
                    if (launched && !shifted && states[leg] >= 0 && worlds.IsAltered != (states[leg] == 1) &&
                        (states[leg - 1] < 0 || motor.VerticalVelocity <= 1f))
                    {
                        while (!worlds.IsReady) yield return null;
                        if (driver != null) requestShift = true;
                        else if (worlds.TrySwitch() != WorldSwitcher.ShiftResult.Accepted)
                        { Fail("Shift rejected on " + roofs[leg]); yield break; }
                        shifted = true;
                    }
                    if (driver != null)
                    {
                        driver.Set(axes, launched, requestShift);
                        yield return null;
                        if (requestShift && worlds.IsAltered != (states[leg] == 1))
                        { Fail("Production input Shift rejected on " + roofs[leg]); yield break; }
                    }
                    else motor.Step(dt, axes, press, false);
                    p = motor.transform.position;
                    if (productionInput && leg == 4 && step % 12 == 0)
                        Debug.Log("SHIFTBOUND PRODUCTION TRACE: p=" + p + " velocity=" + motor.ActualVelocity +
                            " supported=" + motor.IsGrounded + " axes=" + axes + " launched=" + launched);
                    if (launched && motor.IsGrounded && p.x > b.min.x && p.x < b.max.x &&
                        p.z > b.min.z && p.z < b.max.z && p.y >= b.max.y - 0.1f) arrived = true;
                    if (driver == null) yield return new WaitForFixedUpdate();
                    if (motionDirectory != null && frame % 3 == 0)
                    {
                        yield return CaptureYield();
                        CaptureMotionFrame();
                    }
                    if (p.y < -2f || (!flow.IsPlaying && !flow.IsComplete))
                    { Fail("Fell on " + roofs[leg] + " at " + p); yield break; }
                    if (arrived) break;
                }
                if (!arrived) { Fail("Timed out on " + roofs[leg] + " position=" + motor.transform.position +
                    " input=" + motor.input.Move + " dt=" + Time.deltaTime); yield break; }
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GoldenRooftops")
                {
                    // Update may observe contact before the next physics trigger
                    // callback. Await the real checkpoint, rather than calling it.
                    if (leg == 1 && flow.CheckpointSequence == 0)
                    {
                        driver?.Set(Vector2.zero, false);
                        for (int tick = 0; tick < 3 && flow.CheckpointSequence == 0; tick++)
                            yield return new WaitForFixedUpdate();
                    }
                    if ((leg == 1 || leg == 2) && !flow.HasShiftBridgeGuidance)
                    { Fail("First Shift lesson guidance missing before shared landing"); yield break; }
                    if (leg >= 3 && flow.HasShiftBridgeGuidance)
                    { Fail("First Shift lesson guidance persisted beyond its authored exit"); yield break; }
                }
                Debug.Log("SHIFTBOUND ROUTE LEG PASS: " + roofs[leg] + " rate=" + rate + " position=" + motor.transform.position);
            }
            for (int i = 0; i < 500 && !flow.IsComplete; i++, frame++)
            {
                if (driver != null) { driver.Set(Vector2.up * 0.5f, false); yield return null; }
                else
                {
                    motor.Step(StepTime(frame), Vector2.up * 0.5f, false, false);
                    yield return new WaitForFixedUpdate();
                }
                if (motionDirectory != null && frame % 3 == 0)
                {
                    yield return CaptureYield();
                    CaptureMotionFrame();
                }
            }
            if (!flow.IsComplete || flow.CheckpointSequence != 3 || motor.ActualVelocity != Vector3.zero)
            { Fail("Goal/checkpoint triggers or completion motion failed"); yield break; }
            motor.view = oldView;
            yield return null;
            Camera finishCamera = Camera.main;
            Vector3 finishViewport = finishCamera.WorldToViewportPoint(motor.transform.position + Vector3.up);
            Debug.Log("SHIFTBOUND COMPLETION FRAME: player=" + motor.transform.position +
                " camera=" + finishCamera.transform.position + " viewport=" + finishViewport);
            if (finishViewport.z < 1f || finishViewport.x < 0.1f || finishViewport.x > 0.9f ||
                finishViewport.y < 0.1f || finishViewport.y > 0.9f)
            { Fail("Completion camera lost the courier"); yield break; }
            Vector3 finishCameraPosition = finishCamera.transform.position;
            yield return new WaitForSecondsRealtime(0.3f);
            Animator courierAnimator = motor.GetComponentInChildren<Animator>();
            if (courierAnimator == null ||
                courierAnimator.GetCurrentAnimatorClipInfo(0).Length == 0 ||
                courierAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != Animator.StringToHash("Idle") ||
                Vector3.Distance(finishCameraPosition, finishCamera.transform.position) > 0.001f)
            { Fail("Completion animation/camera did not settle into idle/frozen state"); yield break; }
            if (motionDirectory != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    yield return CaptureYield();
                    CaptureMotionFrame();
                    yield return null;
                }
                Time.captureFramerate = 0;
            }
            if (capturePath != "-")
            {
                yield return CaptureYield();
                Capture(Camera.main);
            }
            Debug.Log("SHIFTBOUND FULL ROUTE PASSED: rate=" + rate + " frames=" + frame +
                " input=" + (productionInput ? "production GameInput/Update/camera" : "direct Step") +
                " actual checkpoints=3; actual goal; stopped completion motion.");
            if (driver != null) Debug.Log("SHIFTBOUND PRODUCTION TIMING: requestedRate=" + rate + " " + driver.Timing);
            if(repeatRoute)
            {
                // Explicit device-rendering workload. Uses the complete route,
                // real motor, required airborne Shift, goal and shipped menu.
                // Automated input does not establish physical touch comfort.
                yield return new WaitForSecondsRealtime(2);
                flow.Restart();
                // Retry now retains the actual scene and resources. Reuse the
                // diagnostic input device and continue the telemetry buffer.
                StartCoroutine(Start());yield break;
            }
            Application.Quit(0);
        }
        private void Capture(Camera camera)
        {
            Texture2D image = ReadCameraFrame(camera);
            File.WriteAllBytes(capturePath, image.EncodeToPNG());
            Destroy(image);
        }
        private static object CaptureYield()
        {
            // The batch Editor does not render a Game view/end-of-frame callback.
            // ReadCameraFrame explicitly renders the real camera on the next update.
            return Application.isEditor && Application.isBatchMode ? null : new WaitForEndOfFrame();
        }
        private void CaptureMotionFrame()
        {
            // Hidden batch players may have no presented backbuffer. Render the actual
            // gameplay camera explicitly; this diagnostic excludes the overlay HUD.
            Texture2D frame = ReadCameraFrame(Camera.main);
            File.WriteAllBytes(Path.Combine(motionDirectory, "frame-" + motionFrame.ToString("D5") + ".jpg"), frame.EncodeToJPG(80));
            motionFrame++;
            Destroy(frame);
        }
        private static Texture2D ReadCameraFrame(Camera camera)
        {
            var target = new RenderTexture(1280, 720, 24);
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                GameplayFrameCapture.Render(camera,target);
                RenderTexture.active = target;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                return image;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Destroy(target);
            }
        }
        public static Bounds PlatformBounds(BoxCollider collider)
        {
            // Disabled destination colliders return empty Collider.bounds.
            Vector3 size = Vector3.Scale(collider.size, collider.transform.lossyScale);
            return new Bounds(collider.transform.TransformPoint(collider.center), size);
        }
        private static void Fail(string reason)
        { Debug.LogError("SHIFTBOUND FULL ROUTE FAILED: " + reason); Application.Quit(1); }
    }
}
