using UnityEngine;

namespace Shiftbound
{
    // Pose/contact layer only. Never modifies collision, input, velocity or reach.
    [RequireComponent(typeof(Animator))]
    public sealed class CourierMotionPolish : MonoBehaviour
    {
        public PlayerMotor motor;
        Animator animator;
        Transform chest, leftArm, rightArm, leftForearm, rightForearm;
        Vector3 previousVelocity;
        float compression, lean, bank, contactWeight;
        int landing;
        void Awake()
        {
            animator=GetComponent<Animator>();chest=animator.GetBoneTransform(HumanBodyBones.Chest);
            leftArm=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);rightArm=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftForearm=animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);rightForearm=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
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
            if(landing!=motor.LandingSequence){landing=motor.LandingSequence;compression=Mathf.Clamp(motor.LastLandingSpeed*.007f,0,.085f);}
            compression=Mathf.Lerp(compression,0,1-Mathf.Exp(-18*dt));
            if(chest!=null)chest.localRotation*=Quaternion.Euler(lean,0,bank);
            // The source descent clip splays the arms. Bring elbows close to the
            // delivery bag while preserving the Animator's source joint posture.
            if(!motor.IsGrounded && motor.VerticalVelocity<.6f)
            {
                if(leftArm!=null)leftArm.localRotation*=Quaternion.Euler(0,0,-27);
                if(rightArm!=null)rightArm.localRotation*=Quaternion.Euler(0,0,27);
                if(leftForearm!=null)leftForearm.localRotation*=Quaternion.Euler(0,0,-12);
                if(rightForearm!=null)rightForearm.localRotation*=Quaternion.Euler(0,0,12);
            }
        }
        void OnAnimatorIK(int layer)
        {
            if(motor==null)return;
            contactWeight=Mathf.MoveTowards(contactWeight,motor.IsGrounded?1:0,Time.deltaTime*15);
            Contact(AvatarIKGoal.LeftFoot,HumanBodyBones.LeftFoot);Contact(AvatarIKGoal.RightFoot,HumanBodyBones.RightFoot);
        }
        void Contact(AvatarIKGoal goal,HumanBodyBones bone)
        {
            Transform foot=animator.GetBoneTransform(bone);if(foot==null)return;
            float weight=0;
            if(motor.IsGrounded && Physics.Raycast(foot.position+Vector3.up*.35f,Vector3.down,out RaycastHit hit,.65f,motor.groundMask,QueryTriggerInteraction.Ignore))
            {
                // Only adjust the contact phase; raised swing feet remain authored.
                float height=foot.position.y-hit.point.y;
                weight=contactWeight*(1-Mathf.SmoothStep(.12f,.25f,height));
                animator.SetIKPosition(goal,new Vector3(foot.position.x,hit.point.y+.075f,foot.position.z));
                animator.SetIKRotation(goal,Quaternion.FromToRotation(Vector3.up,hit.normal)*transform.rotation);
            }
            animator.SetIKPositionWeight(goal,weight*.8f);animator.SetIKRotationWeight(goal,weight*.45f);
        }
    }
}
