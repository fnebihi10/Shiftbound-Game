using System;
using System.IO;
using UnityEngine;

namespace Shiftbound
{
    // Opt-in local visual inspection. Readbacks/encoding are excluded from
    // performance measurements. The real motor, animator and camera keep running.
    [DefaultExecutionOrder(500)]
    public sealed class MotionFrameCapture : MonoBehaviour
    {
        string output;
        RenderTexture target;
        Texture2D pixels;
        float start,next;
        int frame;
        StreamWriter times;
        bool district;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-shiftboundMotionCapture");
            if(i>=0&&i+1<args.Length)new GameObject("Opt-in motion frames").AddComponent<MotionFrameCapture>().output=args[i+1];
        }
        void Start()
        {
            Directory.CreateDirectory(output);target=new RenderTexture(1280,720,24);
            district=Array.IndexOf(Environment.GetCommandLineArgs(),"-shiftboundDistrictFrames")>=0;
            if(district)
            {
                var flow=GameFlow.Instance;flow.player.enabled=false;flow.cameraRig.enabled=false;
                flow.player.Teleport(new Vector3(0,.08f,1));flow.player.Step(1f/60,Vector2.zero,false,false);
            }
            pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
            times=new StreamWriter(Path.Combine(output,"timestamps.csv"));times.WriteLine("frame,realtime,grounded,vertical_speed,world");
            start=Time.unscaledTime;next=start+.5f;
        }
        void LateUpdate()
        {
            if(Time.unscaledTime<next)return;
            next=Time.unscaledTime+.1f;
            if(Time.unscaledTime-start>18)
            {
                Debug.Log("SHIFTBOUND MOTION FRAMES COMPLETE: "+frame+" local camera frames; readback overhead, not performance evidence");
                Application.Quit(0);enabled=false;return;
            }
            var camera=Camera.main;if(camera==null)return;
            if(district)
            {
                var flow=GameFlow.Instance;int view=Mathf.FloorToInt((Time.unscaledTime-start)/2)%8;
                bool altered=view>=4;
                if(flow.worlds.IsAltered!=altered && flow.worlds.IsReady)flow.worlds.TrySwitch();
                float[] yaw={0,90,225,180};float[] pitch={18,48,48,35};
                flow.cameraRig.SetInspectionOrbit(yaw[view%4],pitch[view%4]);
            }
            var previous=camera.targetTexture;var active=RenderTexture.active;
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
                File.WriteAllBytes(Path.Combine(output,"frame-"+frame.ToString("D5")+".jpg"),pixels.EncodeToJPG(90));
                var motor=GameFlow.Instance.player;
                times.WriteLine(string.Join(",",frame,Time.unscaledTime.ToString("F6",System.Globalization.CultureInfo.InvariantCulture),motor.IsGrounded,motor.VerticalVelocity.ToString("F3",System.Globalization.CultureInfo.InvariantCulture),GameFlow.Instance.worlds.IsAltered));
                frame++;
            }
            finally{camera.targetTexture=previous;RenderTexture.active=active;}
        }
        void OnDestroy(){times?.Dispose();if(target!=null)Destroy(target);if(pixels!=null)Destroy(pixels);}
    }
}
