using UnityEngine;
using UnityEngine.InputSystem;

namespace Shiftbound
{
    [DefaultExecutionOrder(-350)]
    public sealed class GameInput : MonoBehaviour
    {
        private InputAction move;
        private InputAction mouseLook;
        private InputAction stickLook;
        private InputAction jump;
        private InputAction shift;
        private InputAction pause;
        private InputAction restart;

        public PhoneControls Phone { get; private set; }
        private bool waitingForRelease;
        public Vector2 Move => waitingForRelease ? Vector2.zero : Vector2.ClampMagnitude(move.ReadValue<Vector2>() + Phone.Router.Move, 1f);
        public Vector2 MouseLook => waitingForRelease ? Vector2.zero : mouseLook.ReadValue<Vector2>();
        public Vector2 TouchLook => waitingForRelease ? Vector2.zero : Phone.Router.Look;
        public Vector2 StickLook => waitingForRelease ? Vector2.zero : stickLook.ReadValue<Vector2>();
        public bool JumpPressed => !waitingForRelease && (jump.WasPressedThisFrame() || Phone.Router.JumpPressed);
        public bool JumpReleased => !waitingForRelease && (jump.WasReleasedThisFrame() || Phone.Router.JumpReleased) && !JumpHeld;
        public bool JumpHeld => !waitingForRelease && (jump.IsPressed() || Phone.Router.JumpHeld);
        public bool ShiftPressed => !waitingForRelease && (shift.WasPressedThisFrame() || Phone.Router.ShiftPressed);
        public bool PausePressed => !waitingForRelease && pause.WasPressedThisFrame();
        public bool RestartPressed => !waitingForRelease && restart.WasPressedThisFrame();

        private void Awake()
        {
            Phone = gameObject.AddComponent<PhoneControls>();
            move = new InputAction("Move", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");

            mouseLook = new InputAction("Mouse Look", InputActionType.Value, "<Mouse>/delta");
            stickLook = new InputAction("Stick Look", InputActionType.Value, "<Gamepad>/rightStick");
            jump = Button("Jump", "<Keyboard>/space", "<Gamepad>/buttonSouth");
            shift = Button("Shift", "<Keyboard>/leftShift", "<Keyboard>/rightShift", "<Gamepad>/buttonWest");
            pause = Button("Pause", "<Keyboard>/escape", "<Gamepad>/start");
            restart = Button("Restart", "<Keyboard>/r", "<Gamepad>/select");
        }

        public void CancelGameplayInput()
        {
            Phone.Cancel();
            waitingForRelease = true;
        }

        private void Update()
        {
            // Rearm on a neutral frame; old held controls cannot act on resume.
            if (waitingForRelease && !jump.IsPressed() && !shift.IsPressed() && !pause.IsPressed() && !restart.IsPressed() &&
                move.ReadValue<Vector2>().sqrMagnitude < .01f && !Phone.Router.JumpHeld && Phone.Router.ContactCount == 0)
                waitingForRelease = false;
        }

        private static InputAction Button(string name, params string[] paths)
        {
            var action = new InputAction(name, InputActionType.Button);
            foreach (string path in paths) action.AddBinding(path);
            return action;
        }

        private void OnEnable()
        {
            move?.Enable(); mouseLook?.Enable(); stickLook?.Enable();
            jump?.Enable(); shift?.Enable(); pause?.Enable(); restart?.Enable();
        }

        private void OnDisable()
        {
            move?.Disable(); mouseLook?.Disable(); stickLook?.Disable();
            jump?.Disable(); shift?.Disable(); pause?.Disable(); restart?.Disable();
        }

        private void OnDestroy()
        {
            move?.Dispose(); mouseLook?.Dispose(); stickLook?.Dispose();
            jump?.Dispose(); shift?.Dispose(); pause?.Dispose(); restart?.Dispose();
        }
    }
}
