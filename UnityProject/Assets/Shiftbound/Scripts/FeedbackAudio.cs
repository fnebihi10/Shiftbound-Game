using UnityEngine;

namespace Shiftbound
{
    public sealed class FeedbackAudio : MonoBehaviour
    {
        public AudioSource source;
        private AudioClip success;
        private AudioClip denied;
        private AudioClip checkpoint;
        private AudioClip goal;
        private AudioClip footstep;
        private AudioClip landing;

        private void Awake()
        {
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            success = Tone("Shift", 520f, 0.12f);
            denied = Tone("Blocked", 170f, 0.16f);
            checkpoint = Tone("Checkpoint", 710f, 0.16f);
            goal = Tone("Goal", 900f, 0.35f);
            footstep = Tone("Footstep", 125f, 0.055f);
            landing = Tone("Landing", 95f, 0.13f);
        }

        private static AudioClip Tone(string name, float frequency, float duration)
        {
            const int rate = 22050;
            int length = Mathf.CeilToInt(rate * duration);
            var samples = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / rate;
                float fade = Mathf.Sin(Mathf.PI * i / (length - 1));
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * 0.18f;
            }
            AudioClip clip = AudioClip.Create(name, length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        public void Shift() => source.PlayOneShot(success);
        public void Denied() => source.PlayOneShot(denied);
        public void Checkpoint() => source.PlayOneShot(checkpoint);
        public void Goal() => source.PlayOneShot(goal);
        public void Footstep() => source.PlayOneShot(footstep, 0.45f);
        public void Landing() => source.PlayOneShot(landing, 0.75f);
    }
}
