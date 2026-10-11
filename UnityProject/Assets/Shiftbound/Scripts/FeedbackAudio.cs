using UnityEngine;

namespace Shiftbound
{
    public sealed class FeedbackAudio : MonoBehaviour
    {
        public AudioSource source;
        [Header("Source-backed authored cues (required by scene validation)")]
        public AudioClip shiftClip;
        public AudioClip blockedClip;
        public AudioClip checkpointClip;
        public AudioClip goalClip;
        public AudioClip jumpClip;
        public AudioClip footstepClip;
        public AudioClip[] footstepVariants;
        public AudioClip landingClip;
        public AudioClip hardLandingClip;
        [Header("Optional looping world ambience")]
        public AudioClip presentAmbience;
        public AudioClip overgrownAmbience;
        [Range(0f, 1f)] public float ambienceVolume = 0.25f;
        public WorldSwitcher worlds;
        private AudioSource steps;
        private int stepIndex;
        private AudioSource presentLoop;
        private AudioSource overgrownLoop;

        private void Awake()
        {
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = .65f;
            steps = gameObject.AddComponent<AudioSource>();
            steps.playOnAwake = false;
            steps.volume = .45f;
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

        private void Play(AudioClip clip, float gain = 1f) { if (clip != null) source.PlayOneShot(clip, gain); }
        public void Shift() => Play(shiftClip, .65f);
        public void Denied() => Play(blockedClip, .6f);
        public void Checkpoint() => Play(checkpointClip, .6f);
        public void Goal() => Play(goalClip, .7f);
        public void Jump() => Play(jumpClip, .55f);
        public void Footstep()
        {
            AudioClip clip = footstepClip;
            if (footstepVariants != null && footstepVariants.Length > 0 && stepIndex % (footstepVariants.Length + 1) != 0)
                clip = footstepVariants[stepIndex % (footstepVariants.Length + 1) - 1];
            stepIndex++;
            if (clip != null) steps.PlayOneShot(clip);
        }
        public void Landing(bool hard = false)
        {
            AudioClip clip = landingClip;
            if (hard && hardLandingClip != null) clip = hardLandingClip;
            Play(clip, hard ? .9f : .7f);
        }
    }
}
