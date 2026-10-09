using UnityEngine;
using UnityEngine.InputSystem;

namespace Shiftbound
{
    public sealed class GameInput : MonoBehaviour
    {
        private InputAction move;
        private InputAction mouseLook;
        private InputAction stickLook;
        private InputAction jump;
        private InputAction shift;
        private InputAction pause;
        private InputAction restart;

        public Vector2 Move => move.ReadValue<Vector2>();
        public Vector2 MouseLook => mouseLook.ReadValue<Vector2>();
        public Vector2 StickLook => stickLook.ReadValue<Vector2>();
        public bool JumpPressed => jump.WasPressedThisFrame();
        public bool JumpReleased => jump.WasReleasedThisFrame();
        public bool JumpHeld => jump.IsPressed();
        public bool ShiftPressed => shift.WasPressedThisFrame();
        public bool PausePressed => pause.WasPressedThisFrame();
        public bool RestartPressed => restart.WasPressedThisFrame();

        private void Awake()
        {
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
