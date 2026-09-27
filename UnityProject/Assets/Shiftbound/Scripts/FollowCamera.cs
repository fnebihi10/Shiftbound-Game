using UnityEngine;

namespace Shiftbound
{
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera : MonoBehaviour
    {
        public Transform target;
        public GameInput input;
        public PlayerMotor motor;
        [Range(2f, 10f)] public float distance = 5.4f;
        [Range(0.5f, 3f)] public float lookHeight = 1.65f;
        [Range(0f, 3f)] public float lookAhead = 0f;
        [Range(0.1f, 1f)] public float collisionRadius = 0.26f;
        [Min(1f)] public float positionSharpness = 13f;
        [Min(1f)] public float mouseSensitivity = 0.16f;
        [Min(1f)] public float stickSensitivity = 115f;
        public LayerMask obstacleMask = 1;

        private float yaw;
        private float pitch = 18f;

        private void Start()
        {
            if (target != null) yaw = target.eulerAngles.y;
            Snap();
        }

        private void LateUpdate()
        {
            if (target == null || input == null) return;
            if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) return;
            Vector2 mouse = input.MouseLook;
            Vector2 stick = input.StickLook;
            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
                yaw += mouse.x * mouseSensitivity;
            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
                pitch -= mouse.y * mouseSensitivity;
            yaw += stick.x * stickSensitivity * Time.deltaTime;
            pitch -= stick.y * stickSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -15f, 58f);

            Vector3 focus = target.position + Vector3.up * lookHeight + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * lookAhead;
            if (motor != null) focus += motor.HorizontalVelocity * 0.05f;
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 rayDirection = orbit * Vector3.back;
            float available = distance;
            if (Physics.SphereCast(focus, collisionRadius, rayDirection, out RaycastHit hit,
                distance, obstacleMask, QueryTriggerInteraction.Ignore))
                available = Mathf.Max(0.6f, hit.distance - 0.12f);
            Vector3 wanted = focus + rayDirection * available;
            transform.position = Vector3.Lerp(transform.position, wanted,
                1f - Mathf.Exp(-positionSharpness * Time.deltaTime));
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
        }

        public void Snap()
        {
            if (target == null) return;
            Vector3 focus = target.position + Vector3.up * lookHeight + Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * lookAhead;
            transform.position = focus + Quaternion.Euler(pitch, yaw, 0f) * Vector3.back * distance;
            transform.LookAt(focus);
        }
    }
}

