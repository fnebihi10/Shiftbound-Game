using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Shiftbound
{
    // Opt-in runtime Animator sampling. These renders inspect retargeting and
    // deformation; normal route footage remains the motion/contact evidence.
    public sealed class CourierPoseReview : MonoBehaviour
    {
        string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-shiftboundCourierPoses");
            if(i>=0&&i+1<args.Length)new GameObject("Courier pose review").AddComponent<CourierPoseReview>().output=args[i+1];
        }
        IEnumerator Start()
        {
            yield return null;yield return null;
            Directory.CreateDirectory(output);
            var flow=GameFlow.Instance;flow.player.Teleport(new Vector3(0,.08f,1));flow.player.enabled=false;flow.cameraRig.enabled=false;
            var motion=flow.player.visual.GetComponent<RiggedCourierAnimator>();motion.enabled=false;
            flow.player.visual.GetComponent<CourierMotionPolish>().enabled=false;
            var animator=motion.GetComponent<Animator>();var camera=Camera.main;
            var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;
            using(var csv=new StreamWriter(Path.Combine(output,"feet.csv")))
            {
                csv.WriteLine("state,phase,left_x,left_y,left_z,right_x,right_y,right_z");
                foreach(string state in new[]{"Idle","Walk","Jog","Sprint","Takeoff","Jump","Fall","Land"})
                for(int step=0;step<24;step++)
                {
                    float phase=step/24f;animator.Play("Base Layer."+state,0,phase);animator.Update(0);
                    yield return null;animator.Play("Base Layer."+state,0,phase);animator.Update(0);
                    Vector3 left=motion.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position);
                    Vector3 right=motion.transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
                    csv.WriteLine(string.Join(",",state,phase.ToString(CultureInfo.InvariantCulture),left.x.ToString(CultureInfo.InvariantCulture),left.y.ToString(CultureInfo.InvariantCulture),left.z.ToString(CultureInfo.InvariantCulture),right.x.ToString(CultureInfo.InvariantCulture),right.y.ToString(CultureInfo.InvariantCulture),right.z.ToString(CultureInfo.InvariantCulture)));
                    if(step!=4&&step!=13)continue;
                    foreach(int angle in new[]{0,120,240})
                    {
                        Vector3 focus=motion.transform.position+Vector3.up*1.02f;
                        camera.transform.position=focus+Quaternion.Euler(0,angle,0)*new Vector3(0,.52f,-3.0f);camera.transform.LookAt(focus);
                        Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;
                        var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                        File.WriteAllBytes(Path.Combine(output,state+"-"+step+"-"+angle+".jpg"),image.EncodeToJPG(95));Destroy(image);
                    }
                }
            }
            camera.targetTexture=null;RenderTexture.active=null;Destroy(rt);
            Debug.Log("SHIFTBOUND RUNTIME COURIER POSES PASSED: "+output);Application.Quit(0);
        }
    }
}
