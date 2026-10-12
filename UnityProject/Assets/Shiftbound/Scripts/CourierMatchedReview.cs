using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Shiftbound
{
    [DefaultExecutionOrder(900)]
    public sealed class CourierMatchedReview:MonoBehaviour
    {
        string output;
        Renderer[] baseline,candidate;
        int frames;double next,start;
        int firstFrame;bool finished;
        StreamWriter times;
        Camera closeCamera;
        RiggedCourierAnimator courier;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-shiftboundCourierMatchedReview");
            if(i>=0 && i+1<args.Length)new GameObject("Courier matched evidence").AddComponent<CourierMatchedReview>().output=args[i+1];
        }
        IEnumerator Start()
        {
            var driver=FindFirstObjectByType<RiggedCourierAnimator>();
            courier=driver;
            if(CourierCloseReview.Requested)
            {
                closeCamera=new GameObject("Close evidence camera").AddComponent<Camera>();closeCamera.CopyFrom(Camera.main);closeCamera.enabled=false;closeCamera.fieldOfView=40;
                var source=Camera.main.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                var data=closeCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                data.renderPostProcessing=source.renderPostProcessing;data.renderShadows=source.renderShadows;data.antialiasing=source.antialiasing;data.antialiasingQuality=source.antialiasingQuality;data.volumeLayerMask=source.volumeLayerMask;
            }
            var visual=Instantiate(Resources.Load<GameObject>("CourierBaseline"),driver.transform.parent);
            visual.transform.localPosition=Vector3.zero;
            // Preserve the reference prefab's authored scale. Candidate avatars
            // use different units and must not resize the protected reference.
            visual.transform.localRotation=Quaternion.identity;
            visual.GetComponent<BaselineCourierMotion>().motor=driver.motor;
            candidate=driver.GetComponentsInChildren<Renderer>();baseline=visual.GetComponentsInChildren<Renderer>();
            foreach(var renderer in baseline)renderer.enabled=false;
            Directory.CreateDirectory(output);times=new StreamWriter(Path.Combine(output,"pairs.csv"));
            times.WriteLine("frame,time_s,speed_m_s,grounded,px,py,pz,cx,cy,cz");start=Time.realtimeSinceStartupAsDouble;
            // Fixed simulation only for the comparison. Encode at 30 fps; audio
            // and performance are inspected in a separate ordinary gameplay run.
            Time.captureFramerate=60;firstFrame=Time.frameCount;Application.logMessageReceived+=OnLog;
            while(true)
            {
                yield return new WaitForEndOfFrame();
                if((Time.frameCount-firstFrame)%2!=0)continue;
                double now=Time.realtimeSinceStartupAsDouble;
                Capture("candidate");
                foreach(var r in candidate)r.enabled=false;foreach(var r in baseline)r.enabled=true;
                Capture("baseline");
                foreach(var r in baseline)r.enabled=false;foreach(var r in candidate)r.enabled=true;
                var p=driver.motor.transform.position;var c=(closeCamera?closeCamera:Camera.main).transform.position;
                times.WriteLine(FormattableString.Invariant($"{frames},{(Time.frameCount-firstFrame)/60.0:F6},{driver.motor.ActualVelocity.magnitude:F4},{(driver.motor.IsGrounded?1:0)},{p.x:F5},{p.y:F5},{p.z:F5},{c.x:F5},{c.y:F5},{c.z:F5}"));frames++;
                if(finished){Application.Quit(0);yield break;}
            }
        }
        void OnLog(string message,string stack,LogType type){if(message.StartsWith("SHIFTBOUND FULL ROUTE PASSED:"))finished=true;}
        void Capture(string name)
        {
            var camera=closeCamera?closeCamera:Camera.main;
            if(closeCamera)
            {
                var focus=courier.motor.transform.position+Vector3.up*.90f;
                float orbit=Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundCourierRearReview")>=0?30f:150f;
                closeCamera.transform.position=focus+Quaternion.Euler(14,courier.motor.visual.eulerAngles.y+orbit,0)*Vector3.back*4f;
                closeCamera.transform.LookAt(focus);
            }
            var old=camera.targetTexture;var active=RenderTexture.active;
            var target=RenderTexture.GetTemporary(Screen.width,Screen.height,24);camera.targetTexture=target;
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;
            var image=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(output,name+"-"+frames.ToString("D5")+".jpg"),image.EncodeToJPG(90));
            camera.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(target);Destroy(image);
        }
        void OnApplicationQuit(){Application.logMessageReceived-=OnLog;times?.Dispose();Debug.Log("COURIER MATCHED EVIDENCE COMPLETE: "+frames+" fixed simulation pairs share motor, frame time, camera, resolution and world; visual comparison only, not wall-clock gameplay or a profile.");}
    }
}
