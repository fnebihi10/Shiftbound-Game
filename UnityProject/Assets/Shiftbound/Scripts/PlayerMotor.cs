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
        private float coyoteLeft;
        private float bufferLeft;
        private bool wasGrounded;
        private Vector3 visualBaseScale;
        private bool jumpedSinceGrounded;
        private float stepTravel;

        public Vector3 HorizontalVelocity => horizontalVelocity;
        public Vector3 ActualVelocity { get; private set; }
        public float VerticalVelocity => verticalVelocity;
        public CharacterController Controller => controller;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (visual != null) visualBaseScale = visual.localScale;
        }

        private void Update()
        {
            if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) return;
            Step(Time.deltaTime, input.Move, input.JumpPressed, input.JumpReleased);
        }

        // Also used by the deterministic player smoke check at fixed render steps.
        public void Step(float dt, Vector2 axes, bool jumpPressed, bool jumpReleased)
        {
            bool grounded = verticalVelocity <= 0f && ProbeGround();
            IsGrounded = grounded;
            if (grounded)
            {
                if (!jumpedSinceGrounded || !wasGrounded) coyoteLeft = coyoteTime;
                jumpedSinceGrounded = false;
                if (!wasGrounded && visual != null) visual.localScale = new Vector3(
                    visualBaseScale.x * 1.025f, visualBaseScale.y * 0.96f, visualBaseScale.z * 1.025f);
                if (verticalVelocity < 0f) verticalVelocity = -2f;
            }
            else coyoteLeft -= dt;
            wasGrounded = grounded;

            if (jumpPressed) bufferLeft = jumpBuffer;
            else bufferLeft -= dt;

            if (bufferLeft > 0f && coyoteLeft > 0f)
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
                bufferLeft = 0f;
                coyoteLeft = 0f;
                wasGrounded = false;
                IsGrounded = false;
                jumpedSinceGrounded = true;
                GameFlow.Instance?.feedback?.Jump();
            }
            if (jumpReleased && verticalVelocity > 0f)
                verticalVelocity *= releasedJumpMultiplier;

            Vector3 forward = view != null ? view.forward : Vector3.forward;
            Vector3 right = view != null ? view.right : Vector3.right;
            forward.y = 0f; right.y = 0f;
            Vector3 desired = (forward.normalized * axes.y + right.normalized * axes.x);
            if (desired.sqrMagnitude > 1f) desired.Normalize();
            desired *= maxSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desired,
                (grounded ? groundAcceleration : airAcceleration) * dt);

            verticalVelocity -= gravity * dt;
            Vector3 before = transform.position;
            CollisionFlags flags = controller.Move(
                (horizontalVelocity + Vector3.up * verticalVelocity) * dt);
            ActualVelocity = dt > 0f ? (transform.position - before) / dt : Vector3.zero;
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;
            bool landed = verticalVelocity <= 0f && ProbeGround();
            if (landed)
            {
                if (!grounded && verticalVelocity < -4f)
                    GameFlow.Instance?.feedback?.Landing(verticalVelocity < -11f);
                if (!grounded) coyoteLeft = coyoteTime;
                IsGrounded = true;
                jumpedSinceGrounded = false;
                if (verticalVelocity < 0f) verticalVelocity = -2f;
            }
            else IsGrounded = false;
            wasGrounded = IsGrounded;
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

        private bool ProbeGround()
        {
            float radius = controller.radius * 0.82f;
            Vector3 feet = transform.position + controller.center - Vector3.up *
                (controller.height * 0.5f - controller.radius);
            return Physics.SphereCast(feet + Vector3.up * 0.04f, radius, Vector3.down,
                out RaycastHit hit, controller.radius + groundProbeDistance,
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
            coyoteLeft = 0f;
            bufferLeft = 0f;
            wasGrounded = false;
            IsGrounded = false;
            jumpedSinceGrounded = false;
            ActualVelocity = Vector3.zero;
            stepTravel = 0f;
        }
    }
}
