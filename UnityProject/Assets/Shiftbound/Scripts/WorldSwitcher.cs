using UnityEngine;

namespace Shiftbound
{
    [DefaultExecutionOrder(-50)]
    public sealed class WorldSwitcher : MonoBehaviour
    {
        public enum ShiftResult { Accepted, Blocked, Debounced, NotPlaying }
        public event System.Action<ShiftResult> ShiftAttempted;
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
        private CharacterController controller;
        public bool showLegacyBlockedFlash = true;

        public bool IsAltered => activeWorld == 1;
        public bool IsReady => Time.unscaledTime >= nextSwitch;

        private void Awake()
        {
            controller = playerProbe != null ? playerProbe.GetComponentInParent<CharacterController>() : null;
            if (controller == null || presentRoot == null || alteredRoot == null || ghostMaterial == null)
            {
                Debug.LogError("Shift requires controller, probe, both world roots and ghost material.", this);
                enabled = false;
                return;
            }
            SynchronizeProbe();
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
            if (input != null && input.ShiftPressed)
                TrySwitch();
        }

        public void SynchronizeProbe()
        {
            if (controller == null && playerProbe != null)
                controller = playerProbe.GetComponentInParent<CharacterController>();
            if (controller == null) return;
            playerProbe.transform.SetPositionAndRotation(controller.transform.position, controller.transform.rotation);
            playerProbe.center = controller.center;
            playerProbe.height = controller.height;
            playerProbe.radius = controller.radius;
            playerProbe.direction = 1;
            playerProbe.isTrigger = true;
        }

        public bool DestinationClear(Vector3 controllerPosition, bool altered, out Collider blocker)
        {
            SynchronizeProbe();
            int destination = altered ? 1 : 0;
            blocker = null;
            if (controller == null || collisionSets == null) return false;
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
                        playerProbe, controllerPosition, controller.transform.rotation,
                        obstacle, obstacle.transform.position, obstacle.transform.rotation,
                        out _, out depth) && depth > 0.035f;
                }
                finally
                {
                    obstacle.enabled = wasEnabled;
                }
                if (!blocked) continue;
                blocker = obstacle;
                return false;
            }
            return true;
        }

        public ShiftResult TrySwitch()
        {
            if (!enabled || (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying))
                return Report(ShiftResult.NotPlaying);
            if (!IsReady) return Report(ShiftResult.Debounced);
            // Accepted and blocked attempts share the production timing contract.
            nextSwitch = Time.unscaledTime + switchDebounce;
            if (!DestinationClear(controller.transform.position, !IsAltered, out _))
            {
                rejectionFlash = 0.25f;
                feedback?.Denied();
                GameFlow.Instance?.Notify("SHIFT BLOCKED");
                return Report(ShiftResult.Blocked);
            }
            activeWorld = 1 - activeWorld;
            Apply();
            feedback?.Shift();
            GameFlow.Instance?.Notify(IsAltered ? "OVERGROWN WORLD" : "PRESENT WORLD");
            return Report(ShiftResult.Accepted);
        }

        private ShiftResult Report(ShiftResult result)
        {
            ShiftAttempted?.Invoke(result);
            return result;
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



