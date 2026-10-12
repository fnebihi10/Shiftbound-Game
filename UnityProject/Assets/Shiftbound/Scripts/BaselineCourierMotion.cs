using UnityEngine;

namespace Shiftbound
{
    // Poses follow resolved motor motion. Contact correction never moves the
    // controller and cannot defer an input.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Animator))]
    public sealed class BaselineCourierMotion : MonoBehaviour
    {
        public PlayerMotor motor;
        public float transitionDuration = .10f;
        public float jogThreshold = .25f, sprintThreshold = 4.2f, landingDuration = .18f;
        private Animator animator;
        private Transform chest;
        private int currentState, landingSequence, resetSequence;
        private bool wasGrounded;
        private float airAge, landingAge = 1f, impact, speed, visualSpeed;
        private Vector3 previousVelocity;
        private float previousYaw, leanForward, leanSide;
        private readonly Contact left = new Contact(), right = new Contact();
        private float lastStep = -1f;
        private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
        private static readonly int Locomotion = Animator.StringToHash("Base Layer.Locomotion");
        private static readonly int Air = Animator.StringToHash("Base Layer.Air");
        private static readonly int Speed = Animator.StringToHash("TravelSpeed");
        private static readonly int AirPhase = Animator.StringToHash("AirPhase");
        public float LeftContactWeight => left.weight;
        public float RightContactWeight => right.weight;
        public int ContactStepCount { get; private set; }

        private sealed class Contact
        {
            public bool planted;
            public float weight;
            public Vector3 anchor;
            public Quaternion rotation;
            public bool strikeReady=true;
            public bool releasedUntilLift;
            public void Reset() { planted = false; weight = 0f; strikeReady=true; releasedUntilLift=false; }
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            chest = animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Chest) : null;
            currentState = Idle;
            if (motor != null)
            {
                landingSequence = motor.LandingSequence;
                resetSequence = motor.MotionResetSequence;
                
            }
            previousYaw = transform.eulerAngles.y;
        }

        private void OnDisable() { if (motor != null) { } }
        private void OnEnable() { if (motor != null) { } }

        private void Update()
        {
            if (motor == null || animator.runtimeAnimatorController == null || Time.deltaTime <= 0f) return;
            float dt = Time.deltaTime;
            if (motor.MotionResetSequence != resetSequence)
            {
                resetSequence = motor.MotionResetSequence;
                left.Reset(); right.Reset();
                previousVelocity = Vector3.zero;
                leanForward = leanSide = 0f;
                visualSpeed = 0f;
                landingAge = 1f;
                wasGrounded = false;
                currentState = Idle;
                animator.Play(Idle, 0, 0f);
                previousYaw = transform.eulerAngles.y;
            }
            bool grounded = motor.IsGrounded;
            Vector3 velocity = motor.ActualVelocity; velocity.y = 0f;
            speed = velocity.magnitude;
            visualSpeed = Mathf.Lerp(visualSpeed, speed, 1f - Mathf.Exp(-22f * dt));
            animator.SetFloat(Speed, visualSpeed);
            if (!grounded && wasGrounded)
            {
                airAge = 0f; left.Reset(); right.Reset();
                animator.SetFloat("TakeoffSpeed",speed);
            }
            if (!grounded) airAge += dt;
            if (motor.LandingSequence != landingSequence)
            {
                landingSequence = motor.LandingSequence;
                landingAge = 0f;
                impact = Mathf.InverseLerp(3f, 15f, motor.LastLandingSpeed);
                left.Reset(); right.Reset();
                lastStep = Time.time;
            }
            landingAge += dt;
            wasGrounded = grounded;
            bool complete = GameFlow.Instance != null && GameFlow.Instance.IsComplete;
            int wanted = complete ? Idle : !grounded ? Air : speed > .12f ? Locomotion : Idle;
            if (wanted != currentState)
            {
                animator.CrossFadeInFixedTime(wanted, wanted == Air ? .055f : transitionDuration, 0, 0f);
                currentState = wanted;
            }
            // Extension -> apex tuck -> open landing pose, driven by the actual
            // arc. A released jump also opens early as its velocity changes.
            float phase = motor.VerticalVelocity > 0f
                ? Mathf.Lerp(0f, .5f, Mathf.Clamp01(airAge / Mathf.Max(.1f, Mathf.Sqrt(2f * motor.gravity * motor.jumpHeight) / motor.gravity)))
                : Mathf.Lerp(.5f, 1f, Mathf.Clamp01(-motor.VerticalVelocity / 10f));
            animator.SetFloat(AirPhase, phase);
            float nominal = visualSpeed < 3f ? Mathf.Lerp(1.5f, 3f, Mathf.InverseLerp(1.5f,3f,visualSpeed))
                : Mathf.Lerp(3f, 7f, Mathf.InverseLerp(3f,7f,visualSpeed));
            animator.speed = wanted == Locomotion ? Mathf.Clamp(visualSpeed / nominal, .18f, 1.8f) : 1f;
            Vector3 acceleration = (velocity - previousVelocity) / Mathf.Max(.001f, dt);
            Vector3 localAcceleration = motor.visual.InverseTransformDirection(acceleration);
            float yawRate = Mathf.DeltaAngle(previousYaw, transform.eulerAngles.y) / Mathf.Max(.001f,dt);
            float targetForward = grounded ? Mathf.Clamp(localAcceleration.z * .22f, -8f, 8f) : -2f;
            float targetSide = grounded ? Mathf.Clamp(-yawRate * visualSpeed * .009f, -10f, 10f) : 0f;
            leanForward = Mathf.Lerp(leanForward,targetForward,1f-Mathf.Exp(-10f*dt));
            leanSide = Mathf.Lerp(leanSide,targetSide,1f-Mathf.Exp(-10f*dt));
            previousVelocity = velocity;
            previousYaw = transform.eulerAngles.y;
        }

        private void LateUpdate()
        {
            if (chest != null && Time.deltaTime > 0f)
                chest.localRotation *= Quaternion.Euler(leanForward, 0f, leanSide);
        }

        private void OnAnimatorIK(int layer)
        {
            if (motor == null || !animator.isHuman) return;
            float recovery = motor.IsGrounded ? Mathf.Sin(Mathf.Clamp01(landingAge / .24f) * Mathf.PI) * impact * .075f : 0f;
            float extension=!motor.IsGrounded && motor.VerticalVelocity>0f?Mathf.Sin(Mathf.Clamp01(airAge/.12f)*Mathf.PI)*.035f:0f;
            animator.bodyPosition -= Vector3.up * (recovery+extension);
            SolveContact(left, AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot, animator.leftFeetBottomHeight);
            SolveContact(right, AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot, animator.rightFeetBottomHeight);
        }

        private void SolveContact(Contact contact, AvatarIKGoal goal, HumanBodyBones bone, float sole)
        {
            Transform foot = animator.GetBoneTransform(bone);
            Vector3 pose = foot.position;
            RaycastHit hit = default;
            bool support = motor.IsGrounded && Physics.Raycast(pose + Vector3.up * .30f, Vector3.down,
                out hit, .55f, motor.groundMask, QueryTriggerInteraction.Ignore);
            Vector3 target = support ? hit.point + Vector3.up * sole : pose;
            float lift = pose.y - target.y;
            bool near = support && lift < (contact.planted ? .14f : .085f) && lift > -.15f;
            if(!near){contact.strikeReady=true;contact.releasedUntilLift=false;}
            if (contact.planted && (!near || Vector3.Distance(pose,contact.anchor) > .26f))
            {
                contact.planted=false;
                // Never teleport a fully weighted anchor to a new position.
                // Fade the old contact out, then wait for the swing phase.
                contact.releasedUntilLift=near;
            }
            if (!contact.planted && near && !contact.releasedUntilLift && contact.weight<=.01f)
            {
                contact.planted = true;
                contact.anchor = target;
                contact.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * foot.rotation;
                if (contact.strikeReady && speed > .45f && Time.time-lastStep > .13f)
                {
                    
                    lastStep = Time.time;
                    ContactStepCount++;
                }
                contact.strikeReady=false;
            }
            contact.weight = Mathf.MoveTowards(contact.weight, contact.planted ? 1f : 0f, Time.deltaTime * 18f);
            if (!support) contact.weight = 0f;
            animator.SetIKPositionWeight(goal,contact.weight);
            animator.SetIKRotationWeight(goal,contact.weight * .7f);
            if (contact.weight > 0f)
            {
                animator.SetIKPosition(goal,contact.anchor);
                animator.SetIKRotation(goal,contact.rotation);
            }
        }
    }
}
