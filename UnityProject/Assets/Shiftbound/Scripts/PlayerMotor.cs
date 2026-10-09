using UnityEngine;

namespace Shiftbound
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("References")]
        public GameInput input;
        public Transform view;
        public Transform visual;
        [Header("Movement")]
        [Min(1f)] public float maxSpeed = 7.5f;
        [Min(1f)] public float groundAcceleration = 42f;
        [Min(1f)] public float airAcceleration = 19f;
        [Min(0.1f)] public float turnSpeed = 14f;
        [Header("Jump")]
        [Min(0.1f)] public float jumpHeight = 1.85f;
        [Min(1f)] public float gravity = 28f;
        [Range(0.1f, 1f)] public float releasedJumpMultiplier = 0.45f;
        [Range(0f, 0.4f)] public float coyoteTime = 0.14f;
        [Range(0f, 0.4f)] public float jumpBuffer = 0.14f;
        public LayerMask groundMask = 1;
        [Range(0.02f, 0.25f)] public float groundProbeDistance = 0.12f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private readonly JumpIntent jumpIntent = new JumpIntent();
        private Vector3 visualBaseScale;
        private float stepTravel;
        private float airTime;
        public event System.Action<float> Landed;
        public float LastLandingSpeed { get; private set; }
        public int LandingSequence { get; private set; }
        // Support may bridge the controller's contact skin, never an extra radius.
        public float SupportTolerance => Mathf.Min(groundProbeDistance, controller.skinWidth + 0.02f);

        public Vector3 HorizontalVelocity => horizontalVelocity;
        public Vector3 ActualVelocity { get; private set; }
        public float VerticalVelocity => verticalVelocity;
        public CharacterController Controller => controller;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (view == null && Camera.main != null) view = Camera.main.transform;
            if (visual != null) visualBaseScale = visual.localScale;
        }

        private void Update()
        {
            if (input == null || (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying)) return;
            jumpIntent.SynchronizeHeld(input.JumpHeld);
            Step(Time.deltaTime, input.Move, input.JumpPressed, input.JumpReleased);
        }

        // Also used by the deterministic player smoke check at fixed render steps.
        public void Step(float dt, Vector2 axes, bool jumpPressed, bool jumpReleased)
        {
            if (dt <= 0f || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            bool previouslyGrounded = IsGrounded;
            bool grounded = verticalVelocity <= 0f && ProbeGround(out _);
            if (grounded && !previouslyGrounded) RegisterLanding();
            IsGrounded = grounded;
            if (grounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -2f;
            }
            jumpIntent.Advance(dt, grounded, jumpPressed, jumpReleased, coyoteTime, jumpBuffer);
            bool tookOff = jumpIntent.TryConsume(out bool held);
            if (tookOff)
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight) *
                    (held ? 1f : releasedJumpMultiplier);
                IsGrounded = false;
                GameFlow.Instance?.feedback?.Jump();
            }
            if (!tookOff && jumpReleased && verticalVelocity > 0f)
                verticalVelocity *= releasedJumpMultiplier;

            Vector3 forward = view != null ? view.forward : Vector3.forward;
            Vector3 right = view != null ? view.right : Vector3.right;
            forward.y = 0f; right.y = 0f;
            Vector3 desired = (forward.normalized * axes.y + right.normalized * axes.x);
            if (desired.sqrMagnitude > 1f) desired.Normalize();
            desired *= maxSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desired,
                (grounded ? groundAcceleration : airAcceleration) * dt);

            // Exact constant-gravity displacement avoids reducing the arc at 30 Hz.
            float verticalTravel = verticalVelocity * dt - 0.5f * gravity * dt * dt;
            verticalVelocity -= gravity * dt;
            if (!IsGrounded) airTime += dt;
            Vector3 before = transform.position;
            CollisionFlags flags = controller.Move(
                horizontalVelocity * dt + Vector3.up * verticalTravel);
            ActualVelocity = dt > 0f ? (transform.position - before) / dt : Vector3.zero;
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;
            bool landed = verticalVelocity <= 0f && ProbeGround(out _);
            if (landed)
            {
                if (!IsGrounded) RegisterLanding();
                IsGrounded = true;
                if (verticalVelocity < 0f) verticalVelocity = -2f;
            }
            else IsGrounded = false;
            Vector3 flatTravel = transform.position - before;
            flatTravel.y = 0f;
            if (IsGrounded && flatTravel.sqrMagnitude > 0.0001f)
            {
                stepTravel += flatTravel.magnitude;
                if (stepTravel >= 1.6f)
                {
                    stepTravel = 0f;
                    GameFlow.Instance?.feedback?.Footstep();
                }
            }
            else if (!IsGrounded) stepTravel = 0f;

            if (visual != null)
            {
                if (horizontalVelocity.sqrMagnitude > 0.05f)
                    visual.rotation = Quaternion.Slerp(visual.rotation,
                        Quaternion.LookRotation(horizontalVelocity), turnSpeed * dt);
                visual.localScale = Vector3.Lerp(visual.localScale, visualBaseScale, 12f * dt);
            }
        }

        private void RegisterLanding()
        {
            // Initial spawn/teleport establishes support without an impact cue.
            if (airTime > 0f)
            {
                LastLandingSpeed = Mathf.Max(0f, -verticalVelocity);
                LandingSequence++;
                if (visual != null) visual.localScale = new Vector3(
                    visualBaseScale.x * 1.025f, visualBaseScale.y * 0.96f, visualBaseScale.z * 1.025f);
                Landed?.Invoke(LastLandingSpeed);
                if (LastLandingSpeed > 4f)
                    GameFlow.Instance?.feedback?.Landing(LastLandingSpeed > 11f);
            }
            airTime = 0f;
        }

        public bool ProbeGround(out RaycastHit hit)
        {
            float inset = Mathf.Min(controller.skinWidth * 0.25f, controller.radius * 0.1f);
            float radius = controller.radius - inset;
            const float lift = 0.04f;
            Vector3 feet = transform.position + controller.center - Vector3.up *
                (controller.height * 0.5f - controller.radius);
            return Physics.SphereCast(feet + Vector3.up * lift, radius, Vector3.down,
                out hit, inset + lift + SupportTolerance,
                groundMask, QueryTriggerInteraction.Ignore) &&
                hit.normal.y >= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);
        }

        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            jumpIntent.Reset();
            IsGrounded = false;
            airTime = 0f;
            LastLandingSpeed = 0f;
            ActualVelocity = Vector3.zero;
            stepTravel = 0f;
            if (visual != null) visual.localScale = visualBaseScale;
        }

        public void StopMotion()
        {
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            ActualVelocity = Vector3.zero;
            jumpIntent.Reset();
            stepTravel = 0f;
        }
    }
}
