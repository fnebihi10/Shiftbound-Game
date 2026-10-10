using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace Shiftbound
{
    // Opt-in evidence recorder: actual presented camera/HUD and AudioListener.
    // Readbacks/JPEG encoding materially alter load; never use this as a profile.
    public sealed class ExperienceCapture : MonoBehaviour
    {
        string directory;
        float seconds = 30f;
        readonly List<double> times = new List<double>();
        MixCapture mix;
        bool finished;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args,"-shiftboundExperienceRecord");
            if(i<0 || i+1>=args.Length)return;
            var recorder=new GameObject("Experience evidence").AddComponent<ExperienceCapture>();
            recorder.directory=args[i+1];
            int duration=Array.IndexOf(args,"-shiftboundExperienceSeconds");
            if(duration>=0 && duration+1<args.Length && float.TryParse(args[duration+1],out float value)) recorder.seconds=Mathf.Clamp(value,1f,120f);
        }
        IEnumerator Start()
        {
            Directory.CreateDirectory(directory);
            yield return new WaitForEndOfFrame();
            mix=MixCapture.Create(Path.Combine(directory,"gameplay.wav")); mix.BeginRecording();
            double start=Time.realtimeSinceStartupAsDouble, next=start;
            while(Time.realtimeSinceStartupAsDouble-start<seconds)
            {
                double now=Time.realtimeSinceStartupAsDouble;
                if(now>=next)
                {
                    Texture2D frame;
                    if(Application.isBatchMode)
                    {
                        // Hidden batch players do not present a backbuffer.
                        // Composite the shipped scene and native overlay Canvas.
                        var camera=Camera.main; var target=new RenderTexture(Screen.width,Screen.height,24);
                        var oldTarget=camera.targetTexture; var oldActive=RenderTexture.active;
                        GameplayFrameCapture.Render(camera,target);RenderTexture.active=target;
                        frame=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
                        frame.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0); frame.Apply();
                        camera.targetTexture=oldTarget; RenderTexture.active=oldActive; Destroy(target);
                    }
                    else frame=ScreenCapture.CaptureScreenshotAsTexture();
                    File.WriteAllBytes(Path.Combine(directory,"frame-"+times.Count.ToString("D5")+".jpg"),frame.EncodeToJPG(85));
                    Destroy(frame); times.Add(now-start); next=now+1.0/30.0;
                }
                yield return new WaitForEndOfFrame();
            }
            Finish(); Application.Quit(0);
        }
        void Finish()
        {
            if(finished)return; finished=true;
            mix?.EndRecording();
            using(var csv=new StreamWriter(Path.Combine(directory,"capture-times.csv")))
            using(var concat=new StreamWriter(Path.Combine(directory,"frames.ffconcat")))
            {
                csv.WriteLine("frame,elapsed_s"); concat.WriteLine("ffconcat version 1.0");
                for(int i=0;i<times.Count;i++)
                {
                    csv.WriteLine(i+","+times[i].ToString("F6",CultureInfo.InvariantCulture));
                    concat.WriteLine("file frame-"+i.ToString("D5")+".jpg");
                    double duration=i+1<times.Count?times[i+1]-times[i]:1.0/30.0;
                    concat.WriteLine("duration "+duration.ToString("F6",CultureInfo.InvariantCulture));
                }
                if(times.Count>0)concat.WriteLine("file frame-"+(times.Count-1).ToString("D5")+".jpg");
            }
            Debug.Log("SHIFTBOUND EXPERIENCE CAPTURE COMPLETE: frames="+times.Count+" actual timestamps and mix; NOT performance evidence.");
        }
        void OnApplicationQuit(){Finish();}
    }
}
