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
        [Min(0f)] public float transitionDuration = 0.12f;

        private Animator animator;
        private int currentState;
        private int landingSequence;
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
            landingSequence = motor != null ? motor.LandingSequence : 0;
        }

        private void Update()
        {
            if (motor == null || animator.runtimeAnimatorController == null) return;
            bool grounded = motor.IsGrounded;
            if (motor.LandingSequence != landingSequence)
            {
                if (motor.LastLandingSpeed > 4f) landingUntil = Time.time + landingDuration;
                landingSequence = motor.LandingSequence;
            }

            float speed = new Vector2(motor.ActualVelocity.x, motor.ActualVelocity.z).magnitude;
            bool complete = GameFlow.Instance != null && GameFlow.Instance.IsComplete;
            int wanted = complete ? Idle : !grounded ? Jump : Time.time < landingUntil ? Land :
                speed >= sprintThreshold ? Sprint : speed >= jogThreshold ? Jog : Idle;
            if (wanted != currentState)
            {
                bool gaitChange = (currentState == Jog || currentState == Sprint) &&
                    (wanted == Jog || wanted == Sprint);
                if (gaitChange)
                {
                    float phase = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);
                    animator.CrossFade(wanted, transitionDuration, 0, phase);
                }
                else animator.CrossFadeInFixedTime(wanted, transitionDuration);
                currentState = wanted;
            }
            animator.speed = wanted == Jog ? Mathf.Clamp(speed / 3f, 0.75f, 1.5f) :
                wanted == Sprint ? Mathf.Clamp(speed / 7f, 0.75f, 1.2f) : 1f;
        }
    }
}
