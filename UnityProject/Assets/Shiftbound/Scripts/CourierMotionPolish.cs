using UnityEngine;

namespace Shiftbound
{
    // Pose/contact layer only. Never modifies collision, input, velocity or reach.
    [DefaultExecutionOrder(150)]
    [RequireComponent(typeof(Animator))]
    public sealed class CourierMotionPolish : MonoBehaviour
    {
        public PlayerMotor motor;
        Animator animator;
        Transform chest;
        Transform leftArm,rightArm,leftForearm,rightForearm,leftHand,rightHand;
        Transform leftThigh,rightThigh,leftCalf,rightCalf,leftFoot,rightFoot;
        Vector3 previousVelocity;
        float compression, lean, bank, contactWeight;
        int landing;
        float airWeight;
        float airborneTime;
        bool traceMotion;
        float nextTrace;
        float gaitPhase, gaitWeight;
        Quaternion leftFootBasis,rightFootBasis;
        readonly Vector3[] stancePoint=new Vector3[2];
        readonly bool[] stanceLocked=new bool[2];
        readonly float[] lastPhase=new float[2];
        void Awake()
        {
            animator=GetComponent<Animator>();chest=animator.GetBoneTransform(HumanBodyBones.Chest);
            traceMotion=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-shiftboundMotionTrace")>=0;
            leftArm=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightArm=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftForearm=animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightForearm=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftHand=animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftThigh=animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightThigh=animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            leftCalf=animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rightCalf=animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            leftFoot=animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot=animator.GetBoneTransform(HumanBodyBones.RightFoot);
            leftFootBasis=Quaternion.Inverse(transform.rotation)*leftFoot.rotation;
            rightFootBasis=Quaternion.Inverse(transform.rotation)*rightFoot.rotation;
        }
        void LateUpdate()
        {
            if(motor==null || GameFlow.Instance!=null&&!GameFlow.Instance.IsPlaying)return;
            float dt=Mathf.Max(.001f,Time.deltaTime);
            Vector3 local=transform.InverseTransformDirection(motor.HorizontalVelocity);
            Vector3 acceleration=transform.InverseTransformDirection((motor.HorizontalVelocity-previousVelocity)/dt);
            previousVelocity=motor.HorizontalVelocity;
            lean=Mathf.Lerp(lean,Mathf.Clamp(acceleration.z*.22f,-8f,9f)+local.z*.8f,1-Mathf.Exp(-12*dt));
            bank=Mathf.Lerp(bank,Mathf.Clamp(-acceleration.x*.3f,-12f,12f),1-Mathf.Exp(-10*dt));
            if(landing!=motor.LandingSequence){landing=motor.LandingSequence;compression=Mathf.Clamp(motor.LastLandingSpeed*.004f,0,.06f);}
            compression=Mathf.Lerp(compression,0,1-Mathf.Exp(-18*dt));
            if(chest!=null)chest.localRotation*=Quaternion.Euler(lean+compression*80,0,bank);
            airWeight=Mathf.MoveTowards(airWeight,motor.IsGrounded?0:1,dt*(motor.IsGrounded?12:20));
            airborneTime=motor.IsGrounded?0:airborneTime+dt;
            float speed=new Vector2(motor.ActualVelocity.x,motor.ActualVelocity.z).magnitude;
            if(motor.IsGrounded)gaitPhase=Mathf.Repeat(gaitPhase+dt*RiggedCourierAnimator.GaitCycleRate(speed),1);
            gaitWeight=Mathf.MoveTowards(gaitWeight,motor.IsGrounded&&speed>.15f?1:0,dt*10);
            if(airWeight>0)AirPose();
            if(motor.IsGrounded && gaitWeight>0)
            {
                float swing=Mathf.Sin(gaitPhase*Mathf.PI*2)*Mathf.Lerp(.28f,.65f,Mathf.Clamp01(speed/motor.maxSpeed));
                float weight=gaitWeight*(1-airWeight);
                Aim(leftArm,leftForearm,new Vector3(-.16f,-1,-swing),weight);
                Aim(rightArm,rightForearm,new Vector3(.16f,-1,swing),weight);
                Aim(leftForearm,leftHand,new Vector3(-.04f,-.75f,.28f-swing*.55f),weight);
                Aim(rightForearm,rightHand,new Vector3(.04f,-.75f,.28f+swing*.55f),weight);
            }
            if(traceMotion && Time.time>=nextTrace)
            {
                nextTrace=Time.time+.12f;
                var state=animator.GetCurrentAnimatorStateInfo(0);
                Debug.Log($"SHIFTBOUND MOTION frame={Time.frameCount} grounded={motor.IsGrounded} vy={motor.VerticalVelocity:F2} air={airWeight:F2} state={state.fullPathHash} phase={state.normalizedTime:F2} playback={animator.speed:F2} knee={transform.InverseTransformPoint(leftCalf.position)} foot={transform.InverseTransformPoint(leftFoot.position)}");
            }
        }
        void Aim(Transform from,Transform to,Vector3 direction,float weight)
        {
            if(from==null||to==null)return;
            Vector3 current=to.position-from.position;
            if(current.sqrMagnitude<.000001f)return;
            Quaternion wanted=Quaternion.FromToRotation(current,transform.TransformDirection(direction))*from.rotation;
            from.rotation=Quaternion.Slerp(from.rotation,wanted,weight);
        }
        void AirPose()
        {
            // Derive the pose from the actual ballistic arc, rather than the
            // elapsed time in an imported clip. Uses cached bones/quaternions;
            // no HumanPoseHandler, sampling, allocations or reach changes here.
            float launchSpeed=Mathf.Sqrt(2*motor.gravity*motor.jumpHeight);
            float rise=Mathf.Clamp01(motor.VerticalVelocity/launchSpeed);
            // Keep a recognisable airborne silhouette at the apex. Positive
            // velocity alone collapses to a standing pose halfway through a jump.
            float lift=Mathf.SmoothStep(0,1,Mathf.Clamp01(airborneTime/.12f));
            float reach=Mathf.SmoothStep(0,1,Mathf.Clamp01((-motor.VerticalVelocity/launchSpeed-.45f)/.55f));
            float tuck=lift*Mathf.Lerp(1,.20f,reach);
            float run=Mathf.Clamp01(motor.HorizontalVelocity.magnitude/motor.maxSpeed);
            float armSwing=tuck*.38f*run;
            float spread=Mathf.Lerp(.28f,.16f,rise);
            Aim(leftArm,leftForearm,new Vector3(-spread,-1,.12f-armSwing),airWeight);
            Aim(rightArm,rightForearm,new Vector3(spread,-1,.12f+armSwing),airWeight);
            Aim(leftForearm,leftHand,new Vector3(-.08f,-.35f,.70f+tuck*.25f),airWeight);
            Aim(rightForearm,rightHand,new Vector3(.08f,-.35f,.70f+tuck*.25f),airWeight);
            // Ground contact IK owns the feet immediately after landing. The
            // arm layer eases out while running gait continues without a lock.
            if(motor.IsGrounded)return;
            Quaternion leftRotation=leftFoot!=null?leftFoot.rotation:Quaternion.identity;
            Quaternion rightRotation=rightFoot!=null?rightFoot.rotation:Quaternion.identity;
            Aim(leftThigh,leftCalf,new Vector3(-.06f,-1,.12f+tuck*(.70f+.45f*run)),airWeight);
            Aim(rightThigh,rightCalf,new Vector3(.06f,-1,.10f+tuck*(.60f-.65f*run)),airWeight);
            Aim(leftCalf,leftFoot,new Vector3(0,-1,-.12f-tuck*1.45f),airWeight);
            Aim(rightCalf,rightFoot,new Vector3(0,-1,-.12f-tuck*1.10f),airWeight);
            if(leftFoot!=null)leftFoot.rotation=leftRotation;
            if(rightFoot!=null)rightFoot.rotation=rightRotation;
        }
        void OnAnimatorIK(int layer)
        {
            if(motor==null)return;
            contactWeight=Mathf.MoveTowards(contactWeight,motor.IsGrounded?1:0,Time.deltaTime*15);
            // Absorb a running landing through the hips while foot IK maintains
            // contact. Rig proportions and collider/input remain unchanged.
            if(motor.IsGrounded)animator.bodyPosition-=Vector3.up*(compression*contactWeight);
            GroundStride(AvatarIKGoal.LeftFoot,AvatarIKHint.LeftKnee,-1,gaitPhase,leftFootBasis);
            GroundStride(AvatarIKGoal.RightFoot,AvatarIKHint.RightKnee,1,Mathf.Repeat(gaitPhase+.5f,1),rightFootBasis);
        }
        void GroundStride(AvatarIKGoal goal,AvatarIKHint knee,float side,float phase,Quaternion basis)
        {
            float speed=new Vector2(motor.ActualVelocity.x,motor.ActualVelocity.z).magnitude;
            float rate=RiggedCourierAnimator.GaitCycleRate(speed);
            // 60% stance: the foot travels backwards at the motor's speed.
            // 40% swing: smooth forward recovery with clearance. This replaces
            // the exaggerated crossing/high-kick legs of the inherited clips.
            float span=rate>.01f?Mathf.Min(.62f,speed*.30f/rate):0;
            float swing=phase<.60f?0:(phase-.60f)/.40f;
            float z=phase<.60f?Mathf.Lerp(span,-span,phase/.60f):Mathf.Lerp(-span,span,Mathf.SmoothStep(0,1,swing));
            float lift=Mathf.Sin(swing*Mathf.PI)*Mathf.Lerp(.09f,.23f,Mathf.Clamp01(speed/motor.maxSpeed))*Mathf.SmoothStep(0,1,Mathf.Clamp01(speed/.3f));
            Vector3 target=transform.TransformPoint(new Vector3(side*.115f,.075f+lift,z));
            int index=side<0?0:1;
            if(motor.IsGrounded && phase<.60f && speed>.25f)
            {
                // Keep a planted contact fixed through turning/braking. Release
                // before overstretch, on swing, or on teleport/recovery.
                float reach=Vector3.ProjectOnPlane(stancePoint[index]-transform.position,Vector3.up).sqrMagnitude;
                if(!stanceLocked[index] || lastPhase[index]>=.60f || phase<lastPhase[index] || reach>.81f)
                    stancePoint[index]=target;
                stanceLocked[index]=true;target.x=stancePoint[index].x;target.z=stancePoint[index].z;
            }
            else stanceLocked[index]=false;
            lastPhase[index]=phase;
            float weight=contactWeight;
            if(motor.IsGrounded && Physics.Raycast(target+Vector3.up*.65f,Vector3.down,out RaycastHit hit,1.25f,motor.groundMask,QueryTriggerInteraction.Ignore))
            {
                target.y=hit.point.y+.075f+lift;
                animator.SetIKPosition(goal,target);
                animator.SetIKRotation(goal,Quaternion.FromToRotation(Vector3.up,hit.normal)*transform.rotation*basis);
                animator.SetIKHintPosition(knee,transform.TransformPoint(new Vector3(side*.14f,.55f,.55f)));
            }
            else weight=0;
            animator.SetIKPositionWeight(goal,weight);animator.SetIKRotationWeight(goal,weight*.85f);
            animator.SetIKHintPositionWeight(knee,weight*.85f);
        }
    }
}
