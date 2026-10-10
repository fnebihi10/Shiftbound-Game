using UnityEngine;

namespace Shiftbound
{
    // Pose/contact layer only. Never modifies collision, input, velocity or reach.
    [RequireComponent(typeof(Animator))]
    public sealed class CourierMotionPolish : MonoBehaviour
    {
        public PlayerMotor motor;
        Animator animator;
        Transform chest;
        Vector3 previousVelocity;
        float compression, lean, bank, contactWeight;
        int landing;
        void Awake()
        {
            animator=GetComponent<Animator>();chest=animator.GetBoneTransform(HumanBodyBones.Chest);
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
        }
        void OnAnimatorIK(int layer)
        {
            if(motor==null)return;
            contactWeight=Mathf.MoveTowards(contactWeight,motor.IsGrounded?1:0,Time.deltaTime*15);
            // Absorb a running landing through the hips while foot IK maintains
            // contact. Rig proportions and collider/input remain unchanged.
            if(motor.IsGrounded)animator.bodyPosition-=Vector3.up*(compression*contactWeight);
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
                weight=contactWeight*(1-Mathf.SmoothStep(.24f,.38f,height));
                animator.SetIKPosition(goal,new Vector3(foot.position.x,hit.point.y+.075f,foot.position.z));
                animator.SetIKRotation(goal,Quaternion.FromToRotation(Vector3.up,hit.normal)*transform.rotation);
            }
            animator.SetIKPositionWeight(goal,weight);animator.SetIKRotationWeight(goal,weight*.2f);
        }
    }
}
