using UnityEngine;

namespace Shiftbound
{
    public sealed class FeedbackAudio : MonoBehaviour
    {
        public AudioSource source;
        [Header("Optional authored cues; synthesized tones are fallbacks")]
        public AudioClip shiftClip;
        public AudioClip blockedClip;
        public AudioClip checkpointClip;
        public AudioClip goalClip;
        public AudioClip jumpClip;
        public AudioClip footstepClip;
        public AudioClip landingClip;
        public AudioClip hardLandingClip;
        [Header("Optional looping world ambience")]
        public AudioClip presentAmbience;
        public AudioClip overgrownAmbience;
        [Range(0f, 1f)] public float ambienceVolume = 0.25f;
        public WorldSwitcher worlds;
        private AudioClip success;
        private AudioClip denied;
        private AudioClip checkpoint;
        private AudioClip goal;
        private AudioClip footstep;
        private AudioClip landing;
        private AudioSource presentLoop;
        private AudioSource overgrownLoop;

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
            if (worlds == null) worlds = GetComponent<WorldSwitcher>();
            presentLoop = MakeLoop("Present ambience", presentAmbience);
            overgrownLoop = MakeLoop("Overgrown ambience", overgrownAmbience);
        }

        private AudioSource MakeLoop(string label, AudioClip clip)
        {
            if (clip == null) return null;
            var item = new GameObject(label);
            item.transform.SetParent(transform, false);
            var loop = item.AddComponent<AudioSource>();
            loop.clip = clip;
            loop.loop = true;
            loop.playOnAwake = false;
            loop.volume = 0f;
            loop.Play();
            return loop;
        }

        private void Update()
        {
            float t = 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime);
            bool altered = worlds != null && worlds.IsAltered;
            if (presentLoop != null) presentLoop.volume = Mathf.Lerp(presentLoop.volume,
                altered ? 0f : ambienceVolume, t);
            if (overgrownLoop != null) overgrownLoop.volume = Mathf.Lerp(overgrownLoop.volume,
                altered ? ambienceVolume : 0f, t);
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

        public void Shift() => source.PlayOneShot(shiftClip != null ? shiftClip : success);
        public void Denied() => source.PlayOneShot(blockedClip != null ? blockedClip : denied);
        public void Checkpoint() => source.PlayOneShot(checkpointClip != null ? checkpointClip : checkpoint);
        public void Goal() => source.PlayOneShot(goalClip != null ? goalClip : goal);
        public void Jump() => source.PlayOneShot(jumpClip != null ? jumpClip : success, 0.45f);
        public void Footstep() => source.PlayOneShot(footstepClip != null ? footstepClip : footstep, 0.45f);
        public void Landing(bool hard = false)
        {
            AudioClip clip = landing;
            if (landingClip != null) clip = landingClip;
            if (hard && hardLandingClip != null) clip = hardLandingClip;
            source.PlayOneShot(clip, hard ? 0.9f : 0.7f);
        }
    }
}
