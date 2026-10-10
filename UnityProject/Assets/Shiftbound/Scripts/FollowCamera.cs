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
        // Serialized reference retains the real Android pipeline in comparison
        // builds even when Mobile is excluded from Standalone quality levels.
        public UniversalRenderPipelineAsset mobileDiagnosticPipeline;
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
        private float lastManualOrbit;
        private Camera viewCamera;
        private float restingFov;
        private float focusHeight;
        private bool focusInitialized;
        public float OrbitYaw => yaw;
        public float OrbitPitch => pitch;

        private void Awake()
        {
            viewCamera=GetComponent<Camera>();restingFov=viewCamera.fieldOfView;
            viewCamera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
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
            Vector2 touch = input.TouchLook;
            float invert = PlayerPreferences.InvertY ? -1f : 1f;
            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
                yaw += mouse.x * mouseSensitivity * PlayerPreferences.MouseSensitivity;
            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
                pitch -= mouse.y * mouseSensitivity * PlayerPreferences.MouseSensitivity * invert;
            yaw += stick.x * stickSensitivity * PlayerPreferences.StickSensitivity * Time.deltaTime;
            pitch -= stick.y * stickSensitivity * PlayerPreferences.StickSensitivity * Time.deltaTime * invert;
            yaw += touch.x * .16f * PlayerPreferences.TouchSensitivity;
            pitch += touch.y * .16f * PlayerPreferences.TouchSensitivity * invert;
            bool manual = touch.sqrMagnitude > .01f || stick.sqrMagnitude > .01f ||
                (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.isPressed);
            if (manual) lastManualOrbit = Time.time;
            // Gentle grounded heading follow only. No airborne target switching or
            // reversal snap; release the camera for a full second after manual orbit.
            if (input.Phone.Visible && PlayerPreferences.CameraAssist && !manual && Time.time - lastManualOrbit > 1f &&
                motor != null && motor.IsGrounded && motor.HorizontalVelocity.sqrMagnitude > 4f && input.Move.y > .35f)
            {
                float heading = Mathf.Atan2(motor.HorizontalVelocity.x, motor.HorizontalVelocity.z) * Mathf.Rad2Deg;
                if (Mathf.Abs(Mathf.DeltaAngle(yaw, heading)) < 100f)
                    yaw = Mathf.MoveTowardsAngle(yaw, heading, 35f * Time.deltaTime);
            }
            pitch = Mathf.Clamp(pitch, -15f, 58f);

            // A small, eased speed response gives running room without an
            // abrupt zoom on Jump/Shift. Manual yaw/pitch remain independent.
            float pace=motor!=null?Mathf.Clamp01(motor.HorizontalVelocity.magnitude/motor.maxSpeed):0;
            viewCamera.fieldOfView=Mathf.Lerp(viewCamera.fieldOfView,restingFov+3f*pace,
                1f-Mathf.Exp(-4f*Time.deltaTime));

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
            if(!focusInitialized){focusHeight=focus.y;focusInitialized=true;}
            float followRate=motor!=null&&!motor.IsGrounded&&motor.VerticalVelocity>0?5f:18f;
            focusHeight=Mathf.Lerp(focusHeight,focus.y,1f-Mathf.Exp(-followRate*Time.deltaTime));
            focusHeight=Mathf.Clamp(focusHeight,focus.y-.45f,focus.y+.20f);
            // Keep the landing surface steadier as the courier rises; the
            // bounded lag prevents losing the character on a tall jump.
            focus.y=focusHeight;
            if (motor != null)
            {
                Vector3 velocity = motor.ActualVelocity;
                velocity.y = 0f;
                focus += Vector3.ClampMagnitude(velocity, motor.maxSpeed) * .05f;
            }
            return focus;
        }

        private Vector3 Resolve(Vector3 focus, Vector3 direction, float requested)
        {
            EnsureProbe();
            if (direction.sqrMagnitude < 0.001f) direction = Quaternion.Euler(pitch, yaw, 0f) * Vector3.back;
            float allowed = Mathf.Max(minimumDistance, requested);
            allowed=CameraArchitectureVolume.ClipDistance(focus,direction,allowed,collisionRadius);
            if (Physics.SphereCast(focus, collisionRadius, direction, out RaycastHit hit,
                allowed, obstacleMask, QueryTriggerInteraction.Ignore))
                allowed = Mathf.Max(0.05f, hit.distance - 0.08f);
            Vector3 result = focus + direction * allowed;
            collisionProbe.radius = collisionRadius;
            int count = Physics.OverlapSphereNonAlloc(result, collisionRadius, overlaps,
                obstacleMask, QueryTriggerInteraction.Ignore);
            Collider[] candidates = overlaps;
            // Rare crowded corner fallback: saturation must not silently omit walls.
            if (count == overlaps.Length)
            {
                candidates = Physics.OverlapSphere(result, collisionRadius, obstacleMask, QueryTriggerInteraction.Ignore);
                count = candidates.Length;
            }
            for (int i = 0; i < count; i++)
            {
                Collider obstacle = candidates[i];
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
            focusHeight=target.position.y+lookHeight;focusInitialized=true;
            if(viewCamera!=null)viewCamera.fieldOfView=restingFov;
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

        // Exact inspection pose for paired-world captures; never used by normal input.
        public void SetInspectionOrbit(float inspectionYaw, float inspectionPitch)
        {
            yaw = inspectionYaw;
            pitch = Mathf.Clamp(inspectionPitch, -15f, 58f);
            Snap();
        }
    }
}

