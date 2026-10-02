using UnityEngine;

namespace Shiftbound
{
    [DefaultExecutionOrder(-50)]
    public sealed class WorldSwitcher : MonoBehaviour
    {
        public Transform presentRoot;
        public Transform alteredRoot;
        public CapsuleCollider playerProbe;
        public GameInput input;
        public FeedbackAudio feedback;
        public Material ghostMaterial;
        public Light sun;
        public Color presentLight = new Color(0.83f, 0.91f, 1f);
        public Color alteredLight = new Color(1f, 0.81f, 0.57f);
        [Min(0f)] public float switchDebounce = 0.16f;

        private Collider[][] collisionSets;
        private Renderer[][] renderSets;
        private Material[][][] materialSets;
        private Material[][][] ghostSets;
        private int activeWorld;
        private float nextSwitch;
        private float rejectionFlash;
        public bool showLegacyBlockedFlash = true;

        public bool IsAltered => activeWorld == 1;

        private void Awake()
        {
            collisionSets = new[]
            {
                presentRoot.GetComponentsInChildren<Collider>(true),
                alteredRoot.GetComponentsInChildren<Collider>(true)
            };
            renderSets = new[]
            {
                presentRoot.GetComponentsInChildren<Renderer>(true),
                alteredRoot.GetComponentsInChildren<Renderer>(true)
            };
            materialSets = new Material[2][][];
            ghostSets = new Material[2][][];
            for (int world = 0; world < 2; world++)
            {
                materialSets[world] = new Material[renderSets[world].Length][];
                ghostSets[world] = new Material[renderSets[world].Length][];
                for (int i = 0; i < renderSets[world].Length; i++)
                {
                    materialSets[world][i] = renderSets[world][i].sharedMaterials;
                    ghostSets[world][i] = new Material[materialSets[world][i].Length];
                    for (int slot = 0; slot < ghostSets[world][i].Length; slot++)
                        ghostSets[world][i][slot] = ghostMaterial;
                }
            }
            Apply();
        }

        private void Update()
        {
            if (rejectionFlash > 0f) rejectionFlash -= Time.deltaTime;
            if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) return;
            if (input != null && input.ShiftPressed && Time.unscaledTime >= nextSwitch)
                TrySwitch();
        }

        public void TrySwitch()
        {
            int destination = 1 - activeWorld;
            foreach (Collider obstacle in collisionSets[destination])
            {
                if (obstacle == null || obstacle.isTrigger || !obstacle.gameObject.activeInHierarchy) continue;

                // Unity does not report penetration against a disabled collider. Query it
                // while enabled, then restore its state before any world state is changed.
                bool wasEnabled = obstacle.enabled;
                bool blocked;
                float depth;
                try
                {
                    obstacle.enabled = true;
                    blocked = Physics.ComputePenetration(
                        playerProbe, playerProbe.transform.position, playerProbe.transform.rotation,
                        obstacle, obstacle.transform.position, obstacle.transform.rotation,
                        out _, out depth) && depth > 0.035f;
                }
                finally
                {
                    obstacle.enabled = wasEnabled;
                }
                if (!blocked) continue;
                rejectionFlash = 0.25f;
                feedback?.Denied();
                GameFlow.Instance?.Notify("SHIFT BLOCKED");
                return;
            }
            activeWorld = destination;
            nextSwitch = Time.unscaledTime + switchDebounce;
            Apply();
            feedback?.Shift();
            GameFlow.Instance?.Notify(IsAltered ? "OVERGROWN WORLD" : "PRESENT WORLD");
        }

        private void Apply()
        {
            for (int world = 0; world < 2; world++)
            {
                bool active = world == activeWorld;
                foreach (Collider item in collisionSets[world]) item.enabled = active;
                for (int i = 0; i < renderSets[world].Length; i++)
                {
                    Material[] originals = materialSets[world][i];
                    if (active) renderSets[world][i].sharedMaterials = originals;
                    else renderSets[world][i].sharedMaterials = ghostSets[world][i];
                }
            }
        }

        private void OnGUI()
        {
            if (!showLegacyBlockedFlash || rejectionFlash <= 0f) return;
            GUI.color = new Color(1f, 0.34f, 0.22f, rejectionFlash * 1.5f);
            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), "");
            GUI.color = Color.white;
        }
    }
}



