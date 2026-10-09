using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Shiftbound
{
    // Opt-in standalone checks using real CharacterController/PhysX, never active in normal play.
    public sealed class TraversalRegressionRunner : MonoBehaviour
    {
        public static readonly int[] Rates = { 30, 60, 120, 144 };
        private GameObject fixture;
        private PlayerMotor motor;
        private GameObject floor;
        private const float Origin = 1000f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundRegression") >= 0)
                new GameObject("Traversal regression").AddComponent<TraversalRegressionRunner>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            PlayerMotor scenePlayer = FindFirstObjectByType<PlayerMotor>();
            scenePlayer.enabled = false;
            try
            {
                fixture = new GameObject("Regression fixture");
                var body = new GameObject("Test controller");
                body.SetActive(false);
                body.transform.SetParent(fixture.transform);
                body.layer = 9;
                var controller = body.AddComponent<CharacterController>();
                controller.height = 1.9f; controller.radius = 0.34f;
                controller.center = Vector3.up * 0.95f;
                controller.skinWidth = 0.08f; controller.stepOffset = 0.22f;
                controller.slopeLimit = 55f;
                motor = body.AddComponent<PlayerMotor>(); motor.enabled = false;
                var visual = new GameObject("Test visual"); visual.transform.SetParent(body.transform, false);
                motor.visual = visual.transform;
                body.SetActive(true);
                floor = Box("Test floor", new Vector3(Origin, -0.5f, 0f), new Vector3(30f, 1f, 30f));
                foreach (int rate in Rates) CheckMotor(rate);
                CheckMotor(0); // Repeating variable dt schedule.
                CheckInput();
            }
            catch (Exception error)
            {
                Debug.LogError("SHIFTBOUND REGRESSION FAILED: " + error);
                Cleanup(); Application.Quit(1); yield break;
            }
            Cleanup();
            var worlds = FindFirstObjectByType<WorldSwitcher>();
            // Build a two-slot renderer before Awake caches its materials.
            var slotFixture = new GameObject("Multi-slot Shift fixture"); slotFixture.SetActive(false);
            var actor = new GameObject("Probe actor"); actor.transform.SetParent(slotFixture.transform);
            actor.transform.position = new Vector3(2000f, 0.08f, 0f);
            var shape = actor.AddComponent<CharacterController>();
            shape.height = 1.9f; shape.radius = 0.34f; shape.center = Vector3.up * 0.95f;
            var probeObject = new GameObject("Probe"); probeObject.transform.SetParent(actor.transform, false);
            var probe = probeObject.AddComponent<CapsuleCollider>();
            var present = GameObject.CreatePrimitive(PrimitiveType.Cube);
            present.transform.SetParent(slotFixture.transform); present.transform.position = new Vector3(2003f, 0f, 0f);
            var altered = GameObject.CreatePrimitive(PrimitiveType.Cube);
            altered.transform.SetParent(slotFixture.transform); altered.transform.position = new Vector3(2004f, 0f, 0f);
            var slotRenderer = present.GetComponent<Renderer>();
            Material first = worlds.presentRoot.GetComponentInChildren<Renderer>().sharedMaterial;
            slotRenderer.sharedMaterials = new[] { first, worlds.ghostMaterial };
            var slotWorld = slotFixture.AddComponent<WorldSwitcher>();
            slotWorld.presentRoot = present.transform; slotWorld.alteredRoot = altered.transform;
            slotWorld.playerProbe = probe; slotWorld.ghostMaterial = worlds.ghostMaterial;
            slotFixture.SetActive(true);
            if (slotWorld.TrySwitch() != WorldSwitcher.ShiftResult.Accepted ||
                slotRenderer.sharedMaterials.Length != 2 || slotRenderer.sharedMaterials[0] != worlds.ghostMaterial)
            { Fail("Two-slot ghost materials"); yield break; }
            yield return new WaitForSecondsRealtime(slotWorld.switchDebounce + 0.02f);
            if (slotWorld.TrySwitch() != WorldSwitcher.ShiftResult.Accepted ||
                slotRenderer.sharedMaterials.Length != 2 || slotRenderer.sharedMaterials[0] != first ||
                slotRenderer.sharedMaterials[1] != worlds.ghostMaterial)
            { Fail("Two-slot material restoration"); yield break; }
            slotFixture.SetActive(false); Destroy(slotFixture);
            GameFlow.Instance.TogglePause();
            if (worlds.TrySwitch() != WorldSwitcher.ShiftResult.NotPlaying)
            { Fail("Shift accepted while paused"); yield break; }
            GameFlow.Instance.TogglePause();
            scenePlayer.Teleport(new Vector3(0f, 0.08f, 3f));
            for (int i = 0; i < 8; i++) scenePlayer.Step(1f / 60f, Vector2.up, false, false);
            scenePlayer.Step(1f / 60f, Vector2.up, true, false);
            yield return new WaitForSecondsRealtime(worlds.switchDebounce + 0.02f);
            Vector3 position = scenePlayer.transform.position;
            Vector3 velocity = scenePlayer.HorizontalVelocity;
            float vertical = scenePlayer.VerticalVelocity;
            Vector3 actual = scenePlayer.ActualVelocity;
            Renderer renderer = worlds.presentRoot.GetComponentInChildren<Renderer>();
            Material[] slots = renderer.sharedMaterials;
            bool before = worlds.IsAltered;
            var accepted = worlds.TrySwitch();
            if (accepted != WorldSwitcher.ShiftResult.Accepted || before == worlds.IsAltered ||
                velocity.sqrMagnitude <= 0f || vertical <= 0f ||
                scenePlayer.transform.position != position || scenePlayer.HorizontalVelocity != velocity ||
                scenePlayer.VerticalVelocity != vertical || scenePlayer.ActualVelocity != actual ||
                worlds.TrySwitch() != WorldSwitcher.ShiftResult.Debounced)
            { Fail("Shift continuity/debounce"); yield break; }
            yield return new WaitForSecondsRealtime(worlds.switchDebounce + 0.02f);
            if (worlds.TrySwitch() != WorldSwitcher.ShiftResult.Accepted)
            { Fail("Shift did not become ready"); yield break; }
            Material[] restored = renderer.sharedMaterials;
            if (restored.Length != slots.Length) { Fail("Material slot count changed"); yield break; }
            for (int i = 0; i < slots.Length; i++)
                if (restored[i] != slots[i]) { Fail("Material slot changed"); yield break; }
            scenePlayer.Teleport(new Vector3(2.5f, 0.08f, 3f));
            yield return new WaitForSecondsRealtime(worlds.switchDebounce + 0.02f);
            if (worlds.TrySwitch() != WorldSwitcher.ShiftResult.Blocked || worlds.IsAltered ||
                worlds.TrySwitch() != WorldSwitcher.ShiftResult.Debounced)
            { Fail("Blocked destination/timing"); yield break; }
            // Actual physics callbacks must activate the checkpoint; no direct flow call.
            scenePlayer.Teleport(new Vector3(0f, 0.08f, 9.2f));
            int checkpoints = GameFlow.Instance.CheckpointSequence;
            for (int i = 0; i < 20; i++)
            {
                scenePlayer.Step(0.02f, Vector2.up, false, false);
                yield return new WaitForFixedUpdate();
            }
            if (GameFlow.Instance.CheckpointSequence <= checkpoints)
            { Fail("Checkpoint trigger traversal"); yield break; }
            Vector3 anchor = GameFlow.Instance.CheckpointPosition;
            scenePlayer.Teleport(new Vector3(0f, -20f, 10f));
            GameFlow.Instance.Respawn();
            if (Vector3.Distance(scenePlayer.transform.position, anchor) > 0.001f)
            { Fail("Checkpoint recovery"); yield break; }
            yield return new WaitForSecondsRealtime(worlds.switchDebounce + 0.02f);
            if (worlds.TrySwitch() != WorldSwitcher.ShiftResult.Accepted)
            { Fail("Checkpoint Shift setup"); yield break; }
            GameFlow.Instance.Respawn();
            scenePlayer.Step(0.02f, Vector2.zero, false, false);
            if (!worlds.IsAltered || !scenePlayer.IsGrounded ||
                Vector3.Distance(scenePlayer.transform.forward, Vector3.forward) > 0.001f)
            { Fail("Retry must preserve world, restore facing and support"); yield break; }
            // Exercise keyboard bindings through the normal WorldSwitcher.Update,
            // on actual checkpoint support, including the movement/jump key chord.
            var shiftKeyboard = InputSystem.AddDevice<Keyboard>();
            int keyboardAccepted = 0;
            Action<WorldSwitcher.ShiftResult> recordShift = result =>
            { if (result == WorldSwitcher.ShiftResult.Accepted) keyboardAccepted++; };
            worlds.ShiftAttempted += recordShift;
            bool keyboardShiftPassed = true;
            try
            {
                foreach (Key shiftKey in new[] { Key.LeftShift, Key.RightShift })
                {
                    yield return new WaitForSecondsRealtime(worlds.switchDebounce + 0.02f);
                    bool oldWorld = worlds.IsAltered;
                    int oldAccepted = keyboardAccepted;
                    InputSystem.QueueStateEvent(shiftKeyboard, new KeyboardState(Key.W, Key.Space, shiftKey));
                    yield return null;
                    keyboardShiftPassed &= worlds.IsAltered != oldWorld && keyboardAccepted == oldAccepted + 1;
                    yield return new WaitForSecondsRealtime(worlds.switchDebounce + 0.02f);
                    keyboardShiftPassed &= keyboardAccepted == oldAccepted + 1; // Hold cannot toggle repeatedly.
                    InputSystem.QueueStateEvent(shiftKeyboard, new KeyboardState());
                    yield return null;
                }
            }
            finally
            {
                worlds.ShiftAttempted -= recordShift;
                InputSystem.RemoveDevice(shiftKeyboard);
            }
            if (!keyboardShiftPassed) { Fail("Keyboard Shift through normal Update at checkpoint"); yield break; }
            Debug.Log("SHIFTBOUND KEYBOARD SHIFT PASSED: left/right Shift with W+Space; real checkpoint support; normal WorldSwitcher.Update; one toggle per press.");
            Debug.Log("SHIFTBOUND REGRESSION PASSED: motor cases at 30/60/120/144 Hz and variable dt; Shift timing; checkpoint trigger and recovery. Run route regression for goal traversal.");
            Application.Quit(0);
        }

        private float Dt(int rate, int frame)
        {
            if (rate > 0) return 1f / rate;
            switch (frame % 4) { case 0: return 1f / 30f; case 1: return 1f / 120f;
                case 2: return 1f / 60f; default: return 1f / 144f; }
        }

        private void Reset(Vector3 offset)
        {
            motor.Teleport(new Vector3(Origin, 0f, 0f) + offset);
            Physics.SyncTransforms();
        }

        private void CheckMotor(int rate)
        {
            float dt = Dt(rate, 0);
            Reset(Vector3.up * (motor.SupportTolerance + 0.05f));
            Require(!motor.ProbeGround(out _), "proximity cannot support above tolerance", rate);
            Reset(Vector3.up * 0.06f);
            Require(motor.ProbeGround(out _), "skin-tolerance support", rate);
            float held = JumpPeak(rate, false);
            float tapped = JumpPeak(rate, true);
            Require(held > tapped + 0.5f, "held jump higher than tap", rate);

            Reset(Vector3.up * 0.16f);
            motor.Step(dt, Vector2.zero, true, true);
            bool jumped = false;
            float peak = 0f;
            for (int i = 0; i < 200; i++)
            {
                motor.Step(Dt(rate, i), Vector2.zero, false, false);
                jumped |= motor.VerticalVelocity > 0f;
                peak = Mathf.Max(peak, motor.transform.position.y);
                if (jumped && motor.IsGrounded) break;
            }
            Require(jumped && peak < 0.8f, "released buffered jump remains short", rate);
            Reset(Vector3.up * 3f);
            motor.Step(dt, Vector2.zero, true, true);
            bool expiredJump = false;
            for (int i = 0; i < 400 && !motor.IsGrounded; i++)
            { motor.Step(Dt(rate, i), Vector2.zero, false, false); expiredJump |= motor.VerticalVelocity > 0f; }
            Require(motor.IsGrounded && !expiredJump, "buffer expires before distant landing", rate);

            // Move the support away without resetting intent, as walking off an edge does.
            Reset(Vector3.up * 0.06f); motor.Step(dt, Vector2.zero, false, false);
            floor.SetActive(false); Physics.SyncTransforms();
            motor.Step(dt, Vector2.zero, true, false);
            Require(motor.VerticalVelocity > 0f, "coyote jump inside window", rate);
            floor.SetActive(true); Reset(Vector3.up * 0.06f); motor.Step(dt, Vector2.zero, false, false);
            floor.SetActive(false); Physics.SyncTransforms();
            float elapsed = 0f;
            while (elapsed < motor.coyoteTime + dt)
            { motor.Step(dt, Vector2.zero, false, false); elapsed += dt; }
            motor.Step(dt, Vector2.zero, true, false);
            Require(motor.VerticalVelocity < 0f, "coyote jump outside window", rate);
            floor.SetActive(true);

            var ceiling = Box("Ceiling", new Vector3(Origin, 2.25f, 0f), new Vector3(4f, 0.2f, 4f));
            Reset(Vector3.up * 0.06f); motor.Step(dt, Vector2.zero, false, false);
            motor.Step(dt, Vector2.zero, true, false);
            for (int i = 0; i < 25 && motor.VerticalVelocity > 0f; i++) motor.Step(dt, Vector2.zero, false, false);
            Require(motor.VerticalVelocity <= 0f && motor.transform.position.y < 0.5f, "ceiling cuts ascent", rate);
            ceiling.SetActive(false);

            Reset(new Vector3(15.6f, 0.06f, 0f));
            Require(!motor.ProbeGround(out _), "outside edge is unsupported", rate);
            floor.SetActive(false);
            var slope = Box("Slope", new Vector3(Origin, -0.5f, 0f), new Vector3(8f, 1f, 8f));
            slope.transform.rotation = Quaternion.Euler(0f, 0f, 25f);
            Reset(Vector3.up * 0.04f);
            for (int i = 0; i < 50; i++) motor.Step(dt, Vector2.zero, false, false);
            Require(motor.IsGrounded, "walkable slope support", rate);
            for (int i = 0; i < 12; i++) motor.Step(dt, Vector2.right, false, false);
            Require(motor.IsGrounded, "walkable slope traversal", rate);
            slope.transform.rotation = Quaternion.Euler(0f, 0f, 70f);
            Reset(Vector3.up * 0.8f);
            Require(!motor.ProbeGround(out _), "steep slope not ground", rate);
            slope.SetActive(false); floor.SetActive(true);
            Reset(Vector3.up * .06f);
            float moveTime = 0f;
            for (int i = 0; moveTime < .45f; i++) { float step = Dt(rate, i); motor.Step(step, Vector2.right, false, false); moveTime += step; }
            float reverseTime = 0f;
            for (int i = 0; reverseTime < .4f; i++) { float step = Dt(rate, i); motor.Step(step, Vector2.left, false, false); reverseTime += step; }
            Require(motor.HorizontalVelocity.x < -motor.maxSpeed * .8f && motor.ActualVelocity.x < -1f,
                "reversal responds without animation lockout", rate);
            Reset(Vector3.up * .06f); motor.Step(dt, Vector2.zero, false, false);
            motor.Step(dt, Vector2.up, true, false);
            for (int i = 0; i < 500 && !motor.IsGrounded; i++) motor.Step(Dt(rate,i), Vector2.up, false, false);
            Vector3 landingPosition = motor.transform.position;
            motor.Step(dt, Vector2.up, true, false);
            Require(motor.VerticalVelocity > 0f && motor.transform.position.z > landingPosition.z,
                "landing accepts immediate movement and Jump", rate);
            Reset(Vector3.up * .16f); motor.Step(dt, Vector2.zero, true, false);
            motor.ClearJumpIntent();
            bool resumedJump = false;
            for (int i = 0; i < 200; i++) { motor.Step(Dt(rate,i), Vector2.zero, false, false); resumedJump |= motor.VerticalVelocity > 0f; }
            Require(!resumedJump && motor.IsGrounded, "interruption clears pre-landing jump intent", rate);
            Debug.Log("SHIFTBOUND MOTOR CASES PASS: " + (rate == 0 ? "variable dt" : rate + " Hz") + " heldPeak=" + held + " tapPeak=" + tapped);
        }

        private float JumpPeak(int rate, bool tap)
        {
            Reset(Vector3.up * 0.06f);
            float dt = Dt(rate, 0);
            motor.Step(dt, Vector2.zero, false, false);
            int sequence = motor.LandingSequence;
            motor.Step(dt, Vector2.zero, true, tap);
            float peak = motor.transform.position.y;
            float before = motor.VerticalVelocity;
            motor.Step(dt, Vector2.zero, true, false);
            Require(motor.VerticalVelocity < before, "no repeat jump in ascent", rate);
            for (int i = 0; i < 400 && !motor.IsGrounded; i++)
            { motor.Step(Dt(rate, i), Vector2.zero, false, false); peak = Mathf.Max(peak, motor.transform.position.y); }
            Require(motor.IsGrounded && motor.LandingSequence == sequence + 1 && motor.LastLandingSpeed > 0f,
                "one impact per landing", rate);
            Require(motor.visual.localScale.y < 1f, "post-move landing squash", rate);
            return peak;
        }

        private void CheckInput()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            var inputObject = new GameObject("Input regression");
            var input = inputObject.AddComponent<GameInput>();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space));
                InputSystem.Update();
                Require(input.JumpPressed && input.JumpHeld, "Input System jump press", 0);
                Reset(Vector3.up * 0.16f);
                motor.Step(1f / 60f, Vector2.zero, input.JumpPressed, input.JumpReleased);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                Require(input.JumpReleased && !input.JumpHeld, "Input System jump release", 0);
                motor.Step(1f / 60f, Vector2.zero, input.JumpPressed, input.JumpReleased);
                bool jumped = false;
                for (int i = 0; i < 80; i++)
                {
                    motor.Step(1f / 60f, Vector2.zero, false, false);
                    jumped |= motor.VerticalVelocity > 0f;
                    Require(motor.transform.position.y < 0.8f, "real-input buffered tap remains short", 0);
                }
                Require(jumped, "real-input buffered tap executes", 0);
                Debug.Log("SHIFTBOUND INPUT CASES PASS: virtual keyboard through production InputActions and buffered motor input.");
            }
            finally { inputObject.SetActive(false); Destroy(inputObject); InputSystem.RemoveDevice(keyboard); }
        }

        private GameObject Box(string name, Vector3 position, Vector3 scale)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name; item.transform.SetParent(fixture.transform);
            item.transform.position = position; item.transform.localScale = scale;
            return item;
        }

        private static void Require(bool condition, string label, int rate)
        { if (!condition) throw new Exception(label + " at " + rate + " Hz"); }
        private void Cleanup() { if (fixture != null) { fixture.SetActive(false); Destroy(fixture); } }
        private static void Fail(string reason) { Debug.LogError("SHIFTBOUND REGRESSION FAILED: " + reason); Application.Quit(1); }
    }
}
