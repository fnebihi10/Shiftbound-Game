using UnityEngine;

using UnityEngine.Rendering.Universal;

namespace Shiftbound
{
    [DefaultExecutionOrder(200)]
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
        [Range(0.05f, 1f)] public float minimumDistance = 0.35f;
        [Min(1f)] public float positionSharpness = 13f;
        [Min(0.01f)] public float mouseSensitivity = 0.16f;
        [Min(1f)] public float stickSensitivity = 115f;
        public LayerMask obstacleMask = 1;

        private float yaw;
        private float pitch = 18f;
        private readonly Collider[] overlaps = new Collider[16];
        private SphereCollider collisionProbe;

        private void Awake()
        {
            GetComponent<Camera>().GetUniversalAdditionalCameraData().renderPostProcessing = true;
            EnsureProbe();
        }

        private void EnsureProbe()
        {
            if (collisionProbe != null) return;
            GameObject probe = new GameObject("Camera collision probe");
            probe.hideFlags = HideFlags.DontSave;
            probe.layer = 9;
            probe.transform.SetParent(transform, false);
            collisionProbe = probe.AddComponent<SphereCollider>();
            collisionProbe.isTrigger = true;
        }

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

            Vector3 focus = Focus();
            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 rayDirection = orbit * Vector3.back;
            Vector3 wanted = Resolve(focus, rayDirection, distance);
            Vector3 smoothed = Vector3.Lerp(transform.position, wanted,
                1f - Mathf.Exp(-positionSharpness * Time.deltaTime));
            Vector3 fromFocus = smoothed - focus;
            transform.position = Resolve(focus, fromFocus.normalized,
                Mathf.Min(distance, fromFocus.magnitude));
            transform.rotation = Quaternion.LookRotation(focus - transform.position, Vector3.up);
        }

        private Vector3 Focus()
        {
            Vector3 focus = target.position + Vector3.up * lookHeight +
                Quaternion.Euler(0f, yaw, 0f) * Vector3.forward * lookAhead;
            if (motor != null) focus += motor.ActualVelocity * 0.05f;
            return focus;
        }

        private Vector3 Resolve(Vector3 focus, Vector3 direction, float requested)
        {
            EnsureProbe();
            if (direction.sqrMagnitude < 0.001f) direction = Quaternion.Euler(pitch, yaw, 0f) * Vector3.back;
            float allowed = Mathf.Max(minimumDistance, requested);
            if (Physics.SphereCast(focus, collisionRadius, direction, out RaycastHit hit,
                allowed, obstacleMask, QueryTriggerInteraction.Ignore))
                allowed = Mathf.Max(0.05f, hit.distance - 0.08f);
            Vector3 result = focus + direction * allowed;
            collisionProbe.radius = collisionRadius;
            int count = Physics.OverlapSphereNonAlloc(result, collisionRadius, overlaps,
                obstacleMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                Collider obstacle = overlaps[i];
                if (obstacle == null) continue;
                if (Physics.ComputePenetration(collisionProbe, result, Quaternion.identity,
                    obstacle, obstacle.transform.position, obstacle.transform.rotation,
                    out Vector3 push, out float depth))
                    result += push * (depth + 0.01f);
            }
            return result;
        }

        public void Snap()
        {
            if (target == null) return;
            Vector3 focus = Focus();
            transform.position = Resolve(focus, Quaternion.Euler(pitch, yaw, 0f) * Vector3.back, distance);
            transform.LookAt(focus);
        }

        public void Recover(float facingYaw)
        {
            yaw = facingYaw;
            pitch = 18f;
            Snap();
        }
    }
}

