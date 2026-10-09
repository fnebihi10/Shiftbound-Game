using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Shiftbound
{
    // Opt-in actual AudioListener mix capture. DSP thread only copies into a
    // preallocated buffer; all file work is on the main thread after capture.
    public sealed class MixCapture : MonoBehaviour
    {
        private string output;
        private float[] samples;
        private int used, channels, rate;
        private volatile bool recording;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            string[] args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-shiftboundRecordMix");
            if (i < 0 || i + 1 >= args.Length) return;
            Create(args[i + 1]);
        }
        public static MixCapture Create(string path)
        {
            var capture = FindFirstObjectByType<AudioListener>().gameObject.AddComponent<MixCapture>();
            capture.output = path;
            capture.rate = AudioSettings.outputSampleRate;
            capture.samples = new float[capture.rate * 2 * 180];
            Debug.Log("SHIFTBOUND MIX RECORDER INSTALLED: " + capture.output);
            return capture;
        }
        public void BeginRecording() { recording = true; }
        public void EndRecording() { if (recording) Finish(); }
        private void Update()
        {
            if (!recording && used == 0 && File.Exists(output + ".start"))
            {
                recording = true;
                File.WriteAllText(output + ".ready", DateTime.UtcNow.ToString("o"));
            }
            if (recording && File.Exists(output + ".stop")) Finish();
        }
        private void OnAudioFilterRead(float[] data, int count)
        {
            if (!recording) return;
            channels = count;
            int position = Volatile.Read(ref used);
            int length = Math.Min(data.Length, samples.Length - position);
            Array.Copy(data,0,samples,position,length);
            Volatile.Write(ref used,position+length);
        }
        private void OnApplicationQuit() { if (recording) Finish(); }
        private void Finish()
        {
            recording = false;
            // Snapshot only samples committed before recording stopped; a callback
            // already in progress can append beyond this count, never within it.
            int count = Volatile.Read(ref used);
            if (count == 0 || channels == 0) { Debug.LogError("SHIFTBOUND MIX CAPTURE FAILED: no audio callback"); return; }
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            float peak = 0f; int clipped = 0;
            using (var writer = new BinaryWriter(File.Create(output)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count*2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                writer.Write((short)1); writer.Write((short)channels); writer.Write(rate); writer.Write(rate*channels*2);
                writer.Write((short)(channels*2)); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count*2);
                for (int i=0;i<count;i++) { float value=samples[i]; peak=Mathf.Max(peak,Mathf.Abs(value)); if(Mathf.Abs(value)>=1f)clipped++;
                    writer.Write((short)(Mathf.Clamp(value,-1f,1f)*32767)); }
            }
            File.WriteAllText(output+".txt", "Actual AudioListener PCM mix\nRate="+rate+"\nChannels="+channels+"\nSeconds="+((double)count/channels/rate)+"\nPeak="+peak+"\nClippedSamples="+clipped+"\n");
            Debug.Log("SHIFTBOUND MIX CAPTURE COMPLETE: " + output);
        }
    }
}
