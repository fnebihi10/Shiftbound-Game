using UnityEngine;

namespace Shiftbound
{
    // Poses follow resolved motor motion. Contact correction never moves the
    // controller and cannot defer an input.
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Animator))]
    public sealed class RiggedCourierAnimator : MonoBehaviour
    {
        public PlayerMotor motor;
        public bool authoredActions;
        public bool authoredLocomotion;
        private float gaitPhase;
        private bool plantedYawCorrection=true;
        private bool jumped;
        public float walkCycleDistance=1.397f,runCycleDistance=2.436f,sprintCycleDistance=3.578f;
        private float arcPhase,arcPhaseVelocity,launchVelocity,launchHeight,previousHeight,jumpEnergy=1f;
        public float transitionDuration = .10f;
        public float jogThreshold = .25f, sprintThreshold = 4.2f, landingDuration = .18f;
        private Animator animator;
        private Transform chest;
        private Transform pelvis;
        private float supportCompression,supportCompressionVelocity;
        private Transform pack;
        private Quaternion packBind;
        private float packPitch,packRoll,packPitchVelocity,packRollVelocity;
        private int currentState, landingSequence, resetSequence;
        private bool wasGrounded;
        private float airAge, landingAge = 1f, impact, speed, visualSpeed;
        private Vector3 previousVelocity;
        private float previousYaw, leanForward, leanSide;
        private readonly Contact left = new Contact(), right = new Contact();
        private FootShape leftShape,rightShape;
        [SerializeField] private Vector3[] leftSupportVertices,rightSupportVertices;
        [System.Serializable] private struct SolePoint
        {
            public Vector3 foot,toe;
            public float footWeight,toeWeight;
        }
        [SerializeField] private SolePoint[] leftDeformingSupport,rightDeformingSupport;

        private float lastStep = -1f;
        private static readonly int Idle = Animator.StringToHash("Base Layer.Idle");
        private static readonly int Locomotion = Animator.StringToHash("Base Layer.Locomotion");
        private static readonly int Air = Animator.StringToHash("Base Layer.Air");
        private static readonly int Speed = Animator.StringToHash("TravelSpeed");
        private static readonly int AirPhase = Animator.StringToHash("AirPhase");
        public float LeftContactWeight => left.weight;
        public float RightContactWeight => right.weight;
        public int ContactStepCount { get; private set; }
        public float FootHeadingYaw(bool isLeft)
        {
            var foot=animator.GetBoneTransform(isLeft?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot);
            var toe=animator.GetBoneTransform(isLeft?HumanBodyBones.LeftToes:HumanBodyBones.RightToes);
            var heading=Vector3.ProjectOnPlane(toe!=null?toe.position-foot.position:foot.forward,Vector3.up);
            return heading.sqrMagnitude>.00001f?Vector3.SignedAngle(Vector3.forward,heading,Vector3.up):float.NaN;
        }
        public float LeftSoleDistance=>SoleDistance(leftShape);
        public float RightSoleDistance=>SoleDistance(rightShape);
        private sealed class FootShape { public Transform bone,toe;public Vector3[] vertices;public SolePoint[] points; }

#if UNITY_EDITOR
        public void AuthorSoleSupport()
        {
            animator=GetComponent<Animator>();animator.Rebind();animator.Update(.00001f);
            var l=CacheFootShape(HumanBodyBones.LeftFoot);var r=CacheFootShape(HumanBodyBones.RightFoot);
            if(l==null || r==null)throw new System.InvalidOperationException("Authored courier needs foot/toe support geometry.");
            leftSupportVertices=l.vertices;rightSupportVertices=r.vertices;
            leftDeformingSupport=l.points;rightDeformingSupport=r.points;
            animator.Play("Base Layer.Rest",0,0);animator.Update(.00001f);animator.Update(.00001f);
            var controller=motor.GetComponent<CharacterController>();
            float floor=motor.transform.position.y+controller.center.y-controller.height*.5f-controller.skinWidth;
            float correction=floor-(LowestSole(l).y+LowestSole(r).y)*.5f;
            transform.position+=Vector3.up*correction;
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            Debug.Log("COURIER REST SOLE HEIGHT: correction="+correction+" floor="+floor);
        }        private FootShape CacheFootShape(HumanBodyBones id)
        {
            var bone=animator.GetBoneTransform(id);
            var toe=animator.GetBoneTransform(id==HumanBodyBones.LeftFoot?HumanBodyBones.LeftToes:HumanBodyBones.RightToes);
            var points=new System.Collections.Generic.List<Vector3>();var support=new System.Collections.Generic.List<SolePoint>();
            foreach(var skin in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                int index=System.Array.IndexOf(skin.bones,bone);if(index<0)continue;
                int toeIndex=toe?System.Array.IndexOf(skin.bones,toe):-1;
                var vertices=skin.sharedMesh.vertices;var weights=skin.sharedMesh.boneWeights;var bind=skin.sharedMesh.bindposes[index];
                for(int i=0;i<vertices.Length;i++)
                {
                    var w=weights[i];float footWeight=0,toeWeight=0;
                    void Sum(int joint,float weight){if(joint==index)footWeight+=weight;else if(toeIndex>=0 && joint==toeIndex)toeWeight+=weight;}
                    Sum(w.boneIndex0,w.weight0);Sum(w.boneIndex1,w.weight1);Sum(w.boneIndex2,w.weight2);Sum(w.boneIndex3,w.weight3);
                    if(footWeight>.999f)points.Add(bind.MultiplyPoint3x4(vertices[i]));
                    if(footWeight+toeWeight>.999f)support.Add(new SolePoint{foot=bind.MultiplyPoint3x4(vertices[i]),toe=toeIndex>=0?skin.sharedMesh.bindposes[toeIndex].MultiplyPoint3x4(vertices[i]):Vector3.zero,footWeight=footWeight,toeWeight=toeWeight});
                }
            }
            Debug.Log("COURIER SOLE SUPPORT: "+id+" rigid="+points.Count+" foot/toe="+support.Count);
            return support.Count>0?new FootShape{bone=bone,toe=toe,vertices=points.ToArray(),points=support.ToArray()}:null;
        }
#endif
        private Vector3 LowestSole(FootShape shape)
        {
            var matrix=shape.bone.localToWorldMatrix;Vector3 lowest=new Vector3(0,float.PositiveInfinity,0);
            if(shape.points!=null && shape.points.Length>0)
            {
                var toeMatrix=shape.toe?shape.toe.localToWorldMatrix:Matrix4x4.identity;
                foreach(var point in shape.points){var world=matrix.MultiplyPoint3x4(point.foot)*point.footWeight+toeMatrix.MultiplyPoint3x4(point.toe)*point.toeWeight;if(world.y<lowest.y)lowest=world;}
                return lowest;
            }
            foreach(var vertex in shape.vertices){var world=matrix.MultiplyPoint3x4(vertex);if(world.y<lowest.y)lowest=world;}
            return lowest;
        }
        private float SoleDistance(FootShape shape)
        {
            if(shape==null||motor==null)return float.NaN;
            var point=LowestSole(shape);
            return Physics.Raycast(point+Vector3.up*.3f,Vector3.down,out var hit,.65f,motor.groundMask,QueryTriggerInteraction.Ignore)?point.y-hit.point.y:float.NaN;
        }

        private sealed class Contact
        {
            public bool planted;
            public float weight;
            public Vector3 anchor;
            public Quaternion rotation;
            public Vector3 heading;
            public bool strikeReady=true;
            public bool releasedUntilLift;
            public void Reset() { planted = false; weight = 0f; strikeReady=true; releasedUntilLift=false; }
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
            plantedYawCorrection=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-shiftboundUnconstrainedFootYaw")<0;
            authoredLocomotion |= System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-shiftboundAuthoredGait")>=0;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            chest = animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Chest) : null;
            pelvis=animator.isHuman?animator.GetBoneTransform(HumanBodyBones.Hips):null;
            if(authoredActions && animator.isHuman)
            {
                if(leftSupportVertices!=null && (leftSupportVertices.Length>0 || leftDeformingSupport?.Length>0))leftShape=new FootShape{bone=animator.GetBoneTransform(HumanBodyBones.LeftFoot),toe=animator.GetBoneTransform(HumanBodyBones.LeftToes),vertices=leftSupportVertices,points=leftDeformingSupport};
                if(rightSupportVertices!=null && (rightSupportVertices.Length>0 || rightDeformingSupport?.Length>0))rightShape=new FootShape{bone=animator.GetBoneTransform(HumanBodyBones.RightFoot),toe=animator.GetBoneTransform(HumanBodyBones.RightToes),vertices=rightSupportVertices,points=rightDeformingSupport};
            }
            if(authoredActions)
            {
                foreach(var bone in GetComponentsInChildren<Transform>())if(bone.name=="courier_pack"){pack=bone;packBind=bone.localRotation;break;}
            }
            currentState = authoredActions?Animator.StringToHash("Base Layer.Rest"):Idle;
            previousHeight=motor!=null?motor.transform.position.y:0;
            if (motor != null)
            {
                landingSequence = motor.LandingSequence;
                resetSequence = motor.MotionResetSequence;
                motor.HasContactAudio = true;
            }
            previousYaw = transform.eulerAngles.y;
        }

        private void OnDisable() { if (motor != null) motor.HasContactAudio = false; }
        private void OnEnable() { if (motor != null) motor.HasContactAudio = true; }

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
                gaitPhase=0f;jumped=false;
                arcPhase=arcPhaseVelocity=0;jumpEnergy=1f;previousHeight=motor.transform.position.y;
                packPitch=packRoll=packPitchVelocity=packRollVelocity=0f;
                supportCompression=supportCompressionVelocity=0f;
                landingAge = 1f;
                wasGrounded = false;
                currentState = authoredActions?Animator.StringToHash("Base Layer.Rest"):Idle;
                animator.Play(currentState, 0, 0f);
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
                jumped=motor.VerticalVelocity>1f;
                launchHeight=previousHeight;launchVelocity=Mathf.Max(1,motor.VerticalVelocity);arcPhase=arcPhaseVelocity=0;jumpEnergy=1;
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
                if(authoredActions && authoredLocomotion && speed>.3f)
                {
                    var lf=animator.GetBoneTransform(HumanBodyBones.LeftFoot);var rf=animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    bool leftFirst=lf.position.y<=rf.position.y;
                    float stance=visualSpeed<3.6f?Mathf.Lerp(.55f,.32f,Mathf.InverseLerp(1.5f,3.6f,visualSpeed)):Mathf.Lerp(.32f,.23f,Mathf.InverseLerp(3.6f,7.5f,visualSpeed));
                    float distance=visualSpeed<3.6f?Mathf.Lerp(walkCycleDistance,runCycleDistance,Mathf.InverseLerp(1.5f,3.6f,visualSpeed)):Mathf.Lerp(runCycleDistance,sprintCycleDistance,Mathf.InverseLerp(3.6f,7.5f,visualSpeed));
                    float z=motor.visual.InverseTransformPoint((leftFirst?lf:rf).position).z;
                    float plant=Mathf.Clamp(stance*(.5f-z/(distance*stance)),.03f,stance-.04f);
                    gaitPhase=Mathf.Repeat(plant-(leftFirst?0:.5f),1f);
                }
            }
            landingAge += dt;
            wasGrounded = grounded;
            bool complete = GameFlow.Instance != null && GameFlow.Instance.IsComplete;
            int wanted = complete ? Idle : !grounded ? Air : speed > .12f ? Locomotion : Idle;
            if(authoredActions)
            {
                wanted=Animator.StringToHash("Base Layer."+(complete?"Rest":!grounded?(jumped?"AnimatedArc":"AnimatedFall"):speed>.12f?(authoredLocomotion?"AuthoredGait":"Locomotion"):landingAge<(impact>.45f?.55f:.42f)?(impact>.45f?"HardRecovery":"SoftRecovery"):"Rest"));
            }
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
            if(authoredActions)
            {
                float desired=motor.VerticalVelocity>0?.5f*(1f-Mathf.Clamp01(motor.VerticalVelocity/launchVelocity)):.5f+.5f*Mathf.Clamp01(-motor.VerticalVelocity/Mathf.Max(10f,launchVelocity));
                arcPhase=Mathf.SmoothDamp(arcPhase,Mathf.Max(arcPhase,desired),ref arcPhaseVelocity,.045f,10f,dt);
                animator.SetFloat("JumpTime",Mathf.Clamp01(arcPhase));
                if(!grounded && jumped)
                {
                    float rise=Mathf.Max(0,motor.transform.position.y-launchHeight);
                    float projected=rise+Mathf.Pow(Mathf.Max(0,motor.VerticalVelocity),2)/(2f*motor.gravity);
                    jumpEnergy=Mathf.Lerp(jumpEnergy,Mathf.Clamp01(projected/motor.jumpHeight),1f-Mathf.Exp(-24f*dt));
                }
                animator.SetFloat("JumpEnergy",jumpEnergy);
                // Distances measured from the imported Humanoid stance motion,
                // not the Blender authoring nominal speeds (retargeting changes it).
                float stride=visualSpeed<3.6f?Mathf.Lerp(walkCycleDistance,runCycleDistance,Mathf.InverseLerp(1.5f,3.6f,visualSpeed)):Mathf.Lerp(runCycleDistance,sprintCycleDistance,Mathf.InverseLerp(3.6f,7.5f,visualSpeed));
                stride*=transform.lossyScale.y;
                if(grounded && speed>.12f)gaitPhase=Mathf.Repeat(gaitPhase+speed*dt/stride,1f);
                animator.SetFloat("GaitPhase",gaitPhase);
            }
            float nominal = visualSpeed < 3f ? Mathf.Lerp(1.5f, 3f, Mathf.InverseLerp(1.5f,3f,visualSpeed))
                : Mathf.Lerp(3f, 7f, Mathf.InverseLerp(3f,7f,visualSpeed));
            animator.speed = grounded && speed>.12f && (!authoredActions||!authoredLocomotion) ? Mathf.Clamp(visualSpeed / nominal, .18f, 1.8f) : 1f;
            Vector3 acceleration = (velocity - previousVelocity) / Mathf.Max(.001f, dt);
            Vector3 localAcceleration = motor.visual.InverseTransformDirection(acceleration);
            float yawRate = Mathf.DeltaAngle(previousYaw, transform.eulerAngles.y) / Mathf.Max(.001f,dt);
            float targetForward = grounded ? Mathf.Clamp(localAcceleration.z * .22f, -8f, 8f) : -2f;
            float targetSide = grounded ? Mathf.Clamp(-yawRate * visualSpeed * .009f, -10f, 10f) : 0f;
            leanForward = Mathf.Lerp(leanForward,targetForward,1f-Mathf.Exp(-10f*dt));
            leanSide = Mathf.Lerp(leanSide,targetSide,1f-Mathf.Exp(-10f*dt));
            previousVelocity = velocity;
            previousYaw = transform.eulerAngles.y;
            previousHeight=motor.transform.position.y;
        }

        private void LateUpdate()
        {
            if(authoredActions && motor!=null && animator.isHuman)
            {
                float required=motor.IsGrounded?Mathf.Max(RequiredCompression(left,true),RequiredCompression(right,false)):0f;
                supportCompression=Mathf.SmoothDamp(supportCompression,Mathf.Clamp(required,0,.12f),ref supportCompressionVelocity,required>supportCompression?.015f:.08f,3f,Time.deltaTime);
                if(pelvis!=null)pelvis.position-=Vector3.up*supportCompression;
                SolveContact(left,AvatarIKGoal.LeftFoot,HumanBodyBones.LeftFoot,0);
                SolveContact(right,AvatarIKGoal.RightFoot,HumanBodyBones.RightFoot,0);
            }
            if (chest != null && Time.deltaTime > 0f)
                chest.localRotation *= Quaternion.Euler(leanForward, 0f, leanSide);
            if(pack!=null && Time.deltaTime>0f)
            {
                packPitch=Mathf.SmoothDamp(packPitch,-leanForward*.4f,ref packPitchVelocity,.13f,40f,Time.deltaTime);
                packRoll=Mathf.SmoothDamp(packRoll,-leanSide*.25f,ref packRollVelocity,.16f,35f,Time.deltaTime);
                pack.localRotation=packBind*Quaternion.Euler(Mathf.Clamp(packPitch,-4,4),0,Mathf.Clamp(packRoll,-3,3));
            }
        }

        private void OnAnimatorIK(int layer)
        {
            if (motor == null || !animator.isHuman) return;
            float recovery = motor.IsGrounded ? Mathf.Sin(Mathf.Clamp01(landingAge / .24f) * Mathf.PI) * impact * .075f : 0f;
            float extension=!motor.IsGrounded && motor.VerticalVelocity>0f?Mathf.Sin(Mathf.Clamp01(airAge/.12f)*Mathf.PI)*.035f:0f;
            animator.bodyPosition -= Vector3.up * (recovery+extension);
            if(authoredActions)return;
            SolveContact(left, AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot, animator.leftFeetBottomHeight);
            SolveContact(right, AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot, animator.rightFeetBottomHeight);
        }

        private float RequiredCompression(Contact contact,bool isLeft)
        {
            if(!contact.planted || contact.weight<=.01f)return 0f;
            var upper=animator.GetBoneTransform(isLeft?HumanBodyBones.LeftUpperLeg:HumanBodyBones.RightUpperLeg);
            var lower=animator.GetBoneTransform(isLeft?HumanBodyBones.LeftLowerLeg:HumanBodyBones.RightLowerLeg);
            var foot=animator.GetBoneTransform(isLeft?HumanBodyBones.LeftFoot:HumanBodyBones.RightFoot);
            float reach=Vector3.Distance(upper.position,lower.position)+Vector3.Distance(lower.position,foot.position)-.002f;
            // Foot pitch changes during stop/landing transitions. Refresh the
            // ankle support height before computing reach; yesterday's heel
            // lift must not conceal an unreachable lower target this frame.
            var shape=isLeft?leftShape:rightShape;
            if(shape!=null && Physics.Raycast(new Vector3(contact.anchor.x,foot.position.y+.30f,contact.anchor.z),Vector3.down,out var hit,.65f,motor.groundMask,QueryTriggerInteraction.Ignore))
                contact.anchor.y=hit.point.y+foot.position.y-LowestSole(shape).y;
            var horizontal=contact.anchor-upper.position;horizontal.y=0;
            float height=Mathf.Sqrt(Mathf.Max(0,reach*reach-horizontal.sqrMagnitude));
            // Solve for the weight this frame will acquire, rather than
            // reaching a full plant one frame before the pelvis supports it.
            float projectedWeight=Mathf.Min(1,contact.weight+Time.deltaTime*(authoredLocomotion?32f:18f));
            return Mathf.Max(0,upper.position.y-contact.anchor.y-height)*projectedWeight;
        }

        // Solve against the evaluated pose and real shoe surface. Connected
        // rotations preserve bone lengths; neither the motor nor input is moved.
        private void ApplyLegContact(AvatarIKGoal goal,Transform foot,Vector3 target,Quaternion rotation)
        {
            bool isLeft=goal==AvatarIKGoal.LeftFoot;
            var upper=animator.GetBoneTransform(isLeft?HumanBodyBones.LeftUpperLeg:HumanBodyBones.RightUpperLeg);
            var lower=animator.GetBoneTransform(isLeft?HumanBodyBones.LeftLowerLeg:HumanBodyBones.RightLowerLeg);
            var hip=upper.position;var knee=lower.position;var ankle=foot.position;
            float a=Vector3.Distance(hip,knee),b=Vector3.Distance(knee,ankle);
            var delta=target-hip;float distance=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.0001f,a+b-.0001f);
            var direction=delta.normalized;target=hip+direction*distance;
            var bend=Vector3.ProjectOnPlane(knee-hip,direction);
            if(bend.sqrMagnitude<.00001f)bend=Vector3.ProjectOnPlane(motor.visual.forward,direction);
            float along=(a*a+distance*distance-b*b)/(2f*distance);
            var correctedKnee=hip+direction*along+bend.normalized*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            upper.rotation=Quaternion.FromToRotation(knee-hip,correctedKnee-hip)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(foot.position-lower.position,target-lower.position)*lower.rotation;
            foot.rotation=rotation;
        }
        private void SolveContact(Contact contact, AvatarIKGoal goal, HumanBodyBones bone, float sole)
        {
            Transform foot = animator.GetBoneTransform(bone);
            Vector3 pose = foot.position;
            FootShape shape=goal==AvatarIKGoal.LeftFoot?leftShape:rightShape;
            Quaternion poseRotation=foot.rotation;
            if(shape!=null)sole=pose.y-LowestSole(shape).y;
            RaycastHit hit = default;
            bool support = motor.IsGrounded && Physics.Raycast(pose + Vector3.up * .30f, Vector3.down,
                out hit, .55f, motor.groundMask, QueryTriggerInteraction.Ignore);
            var toe=animator.GetBoneTransform(goal==AvatarIKGoal.LeftFoot?HumanBodyBones.LeftToes:HumanBodyBones.RightToes);
            Vector3 poseHeading=Vector3.ProjectOnPlane(toe!=null?toe.position-foot.position:foot.forward,support?hit.normal:Vector3.up).normalized;
            float pivotAngle=contact.planted && poseHeading.sqrMagnitude>.1f && contact.heading.sqrMagnitude>.1f
                ? Vector3.SignedAngle(contact.heading,poseHeading,hit.normal):0f;
            Vector3 target = support ? hit.point + Vector3.up * sole : pose;
            float lift = pose.y - target.y;
            bool near = support && lift < (contact.planted ? .14f : .085f) && lift > -.15f;
            if(authoredActions && landingAge<.065f)near=support && lift<.3f && lift>-.15f;
            if(authoredActions && authoredLocomotion && speed>.3f && currentState==Animator.StringToHash("Base Layer.AuthoredGait"))
            {
                float stance=visualSpeed<3.6f?Mathf.Lerp(.55f,.32f,Mathf.InverseLerp(1.5f,3.6f,visualSpeed)):Mathf.Lerp(.32f,.23f,Mathf.InverseLerp(3.6f,7.5f,visualSpeed));
                float footPhase=Mathf.Repeat(gaitPhase+(goal==AvatarIKGoal.RightFoot?.5f:0f),1f);
                near=support && footPhase<stance && lift<.16f && lift>-.15f;
            }
            if(!near){contact.strikeReady=true;contact.releasedUntilLift=false;}
            if(authoredActions && speed<.12f && contact.weight<=.01f){contact.releasedUntilLift=false;contact.strikeReady=false;}
            if (contact.planted && (!near || Vector3.Distance(pose,contact.anchor) > .26f || (plantedYawCorrection && Mathf.Abs(pivotAngle)>35f)))
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
                contact.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * poseRotation;
                contact.heading=poseHeading;
                if (contact.strikeReady && speed > .45f && Time.time-lastStep > .13f)
                {
                    GameFlow.Instance?.feedback?.Footstep();
                    lastStep = Time.time;
                    ContactStepCount++;
                }
                contact.strikeReady=false;
            }
            contact.weight = Mathf.MoveTowards(contact.weight, contact.planted ? 1f : 0f, Time.deltaTime * (authoredActions && authoredLocomotion?32f:18f));
            if(shape!=null && support && contact.planted)
            {
                // Keep the horizontal plant while the authored heel/toe roll
                // changes the actual shoe support height. Flat feet are not forced.
                contact.anchor.y=target.y;
                // Preserve heel/toe roll while holding the acquired heading.
                // A larger pivot releases the plant above, avoiding leg twist.
                float headingDelta=plantedYawCorrection && poseHeading.sqrMagnitude>.1f && contact.heading.sqrMagnitude>.1f
                    ? Vector3.SignedAngle(poseHeading,contact.heading,hit.normal):0f;
                contact.rotation=Quaternion.AngleAxis(headingDelta,hit.normal)*Quaternion.FromToRotation(transform.up,hit.normal)*poseRotation;
            }
            if (!support) contact.weight = 0f;
            if(authoredActions)
            {
                Vector3 corrected=Vector3.Lerp(pose,contact.anchor,contact.weight);
                Quaternion correctedRotation=Quaternion.Slerp(poseRotation,contact.rotation,contact.weight);
                // A fading horizontal contact never permits a supported shoe
                // to sink into the roof. This correction lifts only the leg;
                // the motor and animation clock are unaffected.
                bool penetrates=support && corrected.y<target.y;
                if(penetrates)corrected.y=target.y;
                if(contact.weight>0f || penetrates)ApplyLegContact(goal,foot,corrected,correctedRotation);
                return;
            }            animator.SetIKPositionWeight(goal,contact.weight);
            animator.SetIKRotationWeight(goal,contact.weight * .7f);
            if (contact.weight > 0f)
            {
                animator.SetIKPosition(goal,contact.anchor);
                animator.SetIKRotation(goal,contact.rotation);
            }
        }
    }
}
