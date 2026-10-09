using UnityEngine;

namespace Shiftbound
{
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Animator))]
    public sealed class RiggedCourierAnimator : MonoBehaviour
    {
        public PlayerMotor motor;
        [Min(0f)] public float jogThreshold = 0.25f;
        [Min(0f)] public float sprintThreshold = 4.2f;
        [Min(0f)] public float landingDuration = 0.18f;
        [Min(0f)] public float transitionDuration = 0.085f;

        private Animator animator;
        private int currentState;
        private int landingSequence;
        private float landingUntil;
        private bool wasGrounded;
        private float takeoffUntil;
        private static readonly int Walk = Animator.StringToHash("Base Layer.Walk");
        private static readonly int Takeoff = Animator.StringToHash("Base Layer.Takeoff");
        private static readonly int Fall = Animator.StringToHash("Base Layer.Fall");
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
            landingSequence = motor != null ? motor.LandingSequence : 0;
        }

        private void Update()
        {
            if (motor == null || animator.runtimeAnimatorController == null) return;
            bool grounded = motor.IsGrounded;
            if (grounded && !wasGrounded) takeoffUntil = 0f;
            if (!grounded && wasGrounded && motor.VerticalVelocity > 0f) takeoffUntil = Time.time + .09f;
            wasGrounded = grounded;
            if (motor.LandingSequence != landingSequence)
            {
                if (motor.LastLandingSpeed > 4f) landingUntil = Time.time + landingDuration;
                landingSequence = motor.LandingSequence;
            }

            float speed = new Vector2(motor.ActualVelocity.x, motor.ActualVelocity.z).magnitude;
            bool complete = GameFlow.Instance != null && GameFlow.Instance.IsComplete;
            int wanted = complete ? Idle : !grounded ?
                (Time.time < takeoffUntil ? Takeoff : motor.VerticalVelocity > .6f ? Jump : Fall) :
                Time.time < landingUntil && speed < .5f ? Land :
                speed >= sprintThreshold ? Sprint : speed >= 1.8f ? Jog : speed >= jogThreshold ? Walk : Idle;
            if (wanted != currentState)
            {
                bool gaitChange = (currentState == Walk || currentState == Jog || currentState == Sprint) &&
                    (wanted == Walk || wanted == Jog || wanted == Sprint);
                if (gaitChange)
                {
                    float phase = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
                    animator.CrossFade(wanted, transitionDuration, 0, phase);
                }
                else animator.CrossFadeInFixedTime(wanted, transitionDuration, 0,
                    wanted == Takeoff ? .65f : wanted == Fall ? .8f : 0f);
                currentState = wanted;
            }
            animator.speed = wanted == Walk ? Mathf.Clamp(speed / 1.5f, .3f, 1.4f) :
                wanted == Jog ? Mathf.Clamp(speed / 3f, 0.75f, 1.5f) :
                wanted == Sprint ? Mathf.Clamp(speed / 4.8f, 0.7f, 1.9f) : 1f;
        }
    }
}
