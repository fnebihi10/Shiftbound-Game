using System;
using UnityEngine;

namespace Shiftbound
{
    // Anatomical facing, independent of the motor heading and FBX bone axes.
    [DefaultExecutionOrder(600)]
    public sealed class CourierFacingRegression : MonoBehaviour
    {
        PlayerMotor motor; Transform left,right,hips,head; int samples,backwards;
        float minimum=1f;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundFacingRegression")>=0)
                new GameObject("Courier anatomical facing regression").AddComponent<CourierFacingRegression>();
        }
        void Start()
        {
            var courier=FindFirstObjectByType<RiggedCourierAnimator>();motor=courier.motor;
            var animator=courier.GetComponent<Animator>();
            left=animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            right=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            hips=animator.GetBoneTransform(HumanBodyBones.Hips);head=animator.GetBoneTransform(HumanBodyBones.Head);
            Application.logMessageReceived+=OnRoute;
        }
        void LateUpdate()
        {
            if(motor==null || Time.deltaTime<=0f || Time.time<.3f)return;
            Vector3 velocity=motor.ActualVelocity;velocity.y=0;
            if(velocity.magnitude<1f || Vector3.Dot(motor.visual.forward,velocity.normalized)<.95f)return;
            Vector3 facing=Vector3.Cross(right.position-left.position,head.position-hips.position);facing.y=0;
            float alignment=Vector3.Dot(facing.normalized,velocity.normalized);minimum=Mathf.Min(minimum,alignment);samples++;
            backwards=alignment<-.5f?backwards+1:0;
            if(backwards==3)
            {
                Debug.LogError("SHIFTBOUND COURIER FACING FAILED: anatomical front opposes actual travel; dot="+alignment+" speed="+velocity.magnitude);
                Application.Quit(1);
            }
        }
        void OnRoute(string message,string stack,LogType type)
        {
            if(!message.StartsWith("SHIFTBOUND FULL ROUTE PASSED:"))return;
            if(samples>30 && minimum>.5f)Debug.Log("SHIFTBOUND COURIER FACING PASSED: samples="+samples+" minimumDot="+minimum);
            else {Debug.LogError("SHIFTBOUND COURIER FACING FAILED: insufficient correctly facing samples; samples="+samples+" minimumDot="+minimum);Application.Quit(1);}
        }
        void OnDestroy(){Application.logMessageReceived-=OnRoute;}
    }
}
