using UnityEngine;

namespace Shiftbound
{
    [RequireComponent(typeof(Animator))]
    public sealed class RiggedCourierAnimator : MonoBehaviour
    {
        public PlayerMotor motor;
        [Min(0f)] public float jogThreshold = 0.25f;
        [Min(0f)] public float sprintThreshold = 4.2f;
        [Min(0f)] public float landingDuration = 0.18f;
        [Min(0f)] public float transitionDuration = 0.12f;

        private Animator animator;
        private int currentState;
        private bool wasGrounded;
        private float airTime;
        private float landingUntil;
        private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
        private static readonly int Jog = Animator.StringToHash("Base Layer.Jog");
        private static readonly int Sprint = Animator.StringToHash("Base Layer.Sprint");
        private static readonly int Jump = Animator.StringToHash("Base Layer.Jump");
        private static readonly int Land = Animator.StringToHash("Base Layer.Land");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false;
            currentState = Idle;
            wasGrounded = motor != null && motor.Controller.isGrounded;
        }

        private void Update()
        {
            if (motor == null || animator.runtimeAnimatorController == null) return;
            bool grounded = motor.Controller.isGrounded;
            if (!grounded) airTime += Time.deltaTime;
            if (grounded && !wasGrounded && airTime > 0.18f)
                landingUntil = Time.time + landingDuration;
            if (grounded) airTime = 0f;
            wasGrounded = grounded;

            float speed = motor.HorizontalVelocity.magnitude;
            int wanted = !grounded ? Jump : Time.time < landingUntil ? Land :
                speed >= sprintThreshold ? Sprint : speed >= jogThreshold ? Jog : Idle;
            if (wanted != currentState)
            {
                animator.CrossFadeInFixedTime(wanted, transitionDuration);
                currentState = wanted;
            }
            animator.speed = wanted == Jog ? Mathf.Clamp(speed / 3f, 0.75f, 1.5f) : 1f;
        }
    }
}

