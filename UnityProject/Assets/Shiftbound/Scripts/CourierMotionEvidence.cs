using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Shiftbound
{
    // Opt-in post-animation samples. Contact metrics support video inspection;
    // they cannot establish animation quality on their own.
    [DefaultExecutionOrder(500)]
    public sealed class CourierMotionEvidence : MonoBehaviour
    {
        struct Sample { public float time,dt,speed,vertical,leftWeight,rightWeight,leftSole,rightSole;public Vector3 left,right,position;public bool grounded;public int steps; }
        readonly List<Sample> samples=new List<Sample>(10000);
        RiggedCourierAnimator courier;PlayerMotor motor;Transform left,right;string path;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-shiftboundMotionTelemetry");
            if(i>=0 && i+1<args.Length)new GameObject("Courier contact evidence").AddComponent<CourierMotionEvidence>().path=args[i+1];
        }
        void Start()
        {
            courier=FindFirstObjectByType<RiggedCourierAnimator>();motor=courier.motor;var animator=courier.GetComponent<Animator>();
            left=animator.GetBoneTransform(HumanBodyBones.LeftFoot);right=animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
        void LateUpdate()
        {
            if(courier==null || samples.Count>=samples.Capacity)return;
            var velocity=motor.ActualVelocity;velocity.y=0;
            samples.Add(new Sample{time=Time.time,dt=Time.deltaTime,speed=velocity.magnitude,vertical=motor.VerticalVelocity,
                leftWeight=courier.LeftContactWeight,rightWeight=courier.RightContactWeight,left=left.position,right=right.position,
                position=motor.transform.position,grounded=motor.IsGrounded,steps=courier.ContactStepCount,leftSole=courier.LeftSoleDistance,rightSole=courier.RightSoleDistance});
        }
        void OnApplicationQuit()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using(var output=new StreamWriter(path))
            {
                output.WriteLine("time_s,dt_s,speed_m_s,vertical_m_s,grounded,left_weight,right_weight,lx,ly,lz,rx,ry,rz,px,py,pz,contact_steps,left_sole_distance_m,right_sole_distance_m");
                foreach(var s in samples)
                    output.WriteLine(string.Format(CultureInfo.InvariantCulture,"{0:F5},{1:F5},{2:F4},{3:F4},{4},{5:F3},{6:F3},{7:F4},{8:F4},{9:F4},{10:F4},{11:F4},{12:F4},{13:F4},{14:F4},{15:F4},{16},{17:F5},{18:F5}",
                        s.time,s.dt,s.speed,s.vertical,s.grounded?1:0,s.leftWeight,s.rightWeight,s.left.x,s.left.y,s.left.z,s.right.x,s.right.y,s.right.z,s.position.x,s.position.y,s.position.z,s.steps,s.leftSole,s.rightSole));
            }
            Debug.Log("SHIFTBOUND COURIER MOTION EVIDENCE: "+samples.Count+" post-animation samples; audio contacts="+courier.ContactStepCount);
        }
    }
}
