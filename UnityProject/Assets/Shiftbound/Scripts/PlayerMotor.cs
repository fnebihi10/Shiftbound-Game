using UnityEngine;

namespace Shiftbound
{
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

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float coyoteLeft;
        private float bufferLeft;
        private bool wasGrounded;
        private Vector3 visualBaseScale;

        public Vector3 HorizontalVelocity => horizontalVelocity;
        public CharacterController Controller => controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (visual != null) visualBaseScale = visual.localScale;
        }

        private void Update()
        {
            if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) return;
            float dt = Time.deltaTime;
            bool grounded = Physics.CheckSphere(transform.position + Vector3.up * 0.08f, 0.23f,
                groundMask, QueryTriggerInteraction.Ignore);
            if (grounded)
            {
                coyoteLeft = coyoteTime;
                if (!wasGrounded && visual != null) visual.localScale = new Vector3(
                    visualBaseScale.x * 1.14f, visualBaseScale.y * 0.82f, visualBaseScale.z * 1.14f);
                if (verticalVelocity < 0f) verticalVelocity = -2f;
            }
            else coyoteLeft -= dt;
            wasGrounded = grounded;

            if (input.JumpPressed) bufferLeft = jumpBuffer;
            else bufferLeft -= dt;

            if (bufferLeft > 0f && coyoteLeft > 0f)
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
                bufferLeft = 0f;
                coyoteLeft = 0f;
                wasGrounded = false;
            }
            if (input.JumpReleased && verticalVelocity > 0f)
                verticalVelocity *= releasedJumpMultiplier;

            Vector2 axes = input.Move;
            Vector3 forward = view != null ? view.forward : Vector3.forward;
            Vector3 right = view != null ? view.right : Vector3.right;
            forward.y = 0f; right.y = 0f;
            Vector3 desired = (forward.normalized * axes.y + right.normalized * axes.x);
            if (desired.sqrMagnitude > 1f) desired.Normalize();
            desired *= maxSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desired,
                (grounded ? groundAcceleration : airAcceleration) * dt);

            verticalVelocity -= gravity * dt;
            CollisionFlags flags = controller.Move(
                (horizontalVelocity + Vector3.up * verticalVelocity) * dt);
            if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
                verticalVelocity = 0f;

            if (visual != null)
            {
                if (horizontalVelocity.sqrMagnitude > 0.05f)
                    visual.rotation = Quaternion.Slerp(visual.rotation,
                        Quaternion.LookRotation(horizontalVelocity), turnSpeed * dt);
                visual.localScale = Vector3.Lerp(visual.localScale, visualBaseScale, 12f * dt);
            }
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
        }
    }
}
