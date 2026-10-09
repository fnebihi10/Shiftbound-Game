using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Shiftbound
{
    // Added only by the opt-in route diagnostic. The real motor/camera stay enabled.
    [DefaultExecutionOrder(-300)]
    public sealed class ProductionInputDriver : MonoBehaviour
    {
        private Gamepad pad;
        private Vector2 worldAxes;
        private bool jumpHeld;
        private bool shiftRequested;
        private Vector2 orbitAxes;
        public int editorFrameRate = 60;
        private int frames;
        private float totalDt;
        private float minDt = float.PositiveInfinity;
        private float maxDt;
#if UNITY_EDITOR
        private InputSettings originalSettings;
        private InputSettings testSettings;
#endif
        public string Timing => "samples=" + frames + " meanDt=" + (totalDt / Mathf.Max(1, frames)).ToString("F5") +
            " minDt=" + minDt.ToString("F5") + " maxDt=" + maxDt.ToString("F5");

        private void Awake()
        {
#if UNITY_EDITOR
            originalSettings = InputSystem.settings;
            testSettings = Instantiate(originalSettings);
            testSettings.hideFlags = HideFlags.DontSave;
            testSettings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            testSettings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings = testSettings;
#endif
            pad = InputSystem.AddDevice<Gamepad>();
        }

        public void Set(Vector2 axes, bool jump, bool shift = false)
        { worldAxes = axes; jumpHeld = jump; shiftRequested |= shift; }
        public void SetLook(Vector2 axes) { orbitAxes = axes; }

        private void Update()
        {
#if UNITY_EDITOR
            // The batch Editor ignores Application.targetFrameRate. Pace this opt-in
            // harness with real time so gameplay and unscaled cooldown clocks agree.
            System.Threading.Thread.Sleep(Mathf.Max(1, 1000 / editorFrameRate));
#endif
            frames++;
            totalDt += Time.deltaTime;
            minDt = Mathf.Min(minDt, Time.deltaTime);
            maxDt = Mathf.Max(maxDt, Time.deltaTime);
            Transform view = Camera.main.transform;
            Vector3 forward = view.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = view.right; right.y = 0f; right.Normalize();
            Vector3 direction = new Vector3(worldAxes.x, 0f, worldAxes.y);
            var state = new GamepadState { leftStick = new Vector2(
                Vector3.Dot(direction, right), Vector3.Dot(direction, forward)), rightStick = orbitAxes };
            if (jumpHeld) state = state.WithButton(GamepadButton.South);
            if (shiftRequested) state = state.WithButton(GamepadButton.West);
            InputSystem.QueueStateEvent(pad, state);
            // Consume the synthetic event before the normal GameInput/motor Update.
            InputSystem.Update();
            shiftRequested = false;
        }

        private void OnDestroy()
        {
            if (pad != null) InputSystem.RemoveDevice(pad);
#if UNITY_EDITOR
            if (originalSettings != null) InputSystem.settings = originalSettings;
            if (testSettings != null) Destroy(testSettings);
#endif
        }
    }
}
