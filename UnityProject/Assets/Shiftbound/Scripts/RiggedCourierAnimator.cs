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
        private float walkCycle, jogCycle, sprintCycle;
        private int takeoffSequence;
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
            takeoffSequence = motor != null ? motor.TakeoffSequence : 0;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.Contains("Walk")) walkCycle = clip.length;
                if (clip.name.Contains("Jog")) jogCycle = clip.length;
                if (clip.name.Contains("Sprint") || clip.name.Contains("Run")) sprintCycle = clip.length;
            }
            if (walkCycle <= 0) walkCycle = .8f;
            if (jogCycle <= 0) jogCycle = .7f;
            if (sprintCycle <= 0) sprintCycle = .65f;
        }

        private void Update()
        {
            if (motor == null || animator.runtimeAnimatorController == null) return;
            bool grounded = motor.IsGrounded;
            if (grounded && !wasGrounded) takeoffUntil = 0f;
            // A buffered jump can launch on the same motor step as contact.
            // Use the actual launch event rather than missing that transition.
            if (motor.TakeoffSequence != takeoffSequence)
            {
                takeoffSequence = motor.TakeoffSequence;
                takeoffUntil = Time.time + .075f;
                landingUntil = 0f;
            }
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
                else animator.CrossFadeInFixedTime(wanted,
                    wanted == Takeoff ? .025f : wanted == Fall ? .10f :
                    wanted == Land ? .045f : transitionDuration, 0, 0f);
                currentState = wanted;
            }
            // Distance travelled per authored two-contact cycle, rather than
            // an arbitrary nominal speed which changes with the clip length.
            // Runtime avatar foot samples: stance travel per normalized cycle
            // is about1.42m in Walk and5.2m in these running source clips.
            animator.speed = wanted == Walk ? Mathf.Clamp(speed * walkCycle / 1.42f, .15f, 2f) :
                wanted == Jog ? Mathf.Clamp(speed * jogCycle / 5.2f, .2f, 2f) :
                wanted == Sprint ? Mathf.Clamp(speed * sprintCycle / 5.2f, .3f, 2.3f) : 1f;
        }
    }
}
