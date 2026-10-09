using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftbound
{
    [DefaultExecutionOrder(-200)]
    public sealed class GameFlow : MonoBehaviour
    {
        public static GameFlow Instance { get; private set; }

        public GameInput input;
        public PlayerMotor player;
        public FollowCamera cameraRig;
        public FeedbackAudio feedback;
        public WorldSwitcher worlds;
        public float fallHeight = -12f;
        private Vector3 checkpoint;
        private float checkpointYaw;
        private Transform shiftBridgeLessonExit;
        private Vector3 shiftBridgeLessonEnd;
        public bool HasShiftBridgeGuidance
        {
            get
            {
                if (!IsPlaying || shiftBridgeLessonExit == null) return false;
                // Authored exit defines the forward progress plane; retry restores
                // guidance naturally, without a timer or changing the world state.
                Vector3 route = shiftBridgeLessonEnd - checkpoint;
                route.y = 0f;
                Vector3 progress = player.transform.position - checkpoint;
                progress.y = 0f;
                return route.sqrMagnitude > 0.01f && Vector3.Dot(progress, route) < route.sqrMagnitude;
            }
        }
        public Vector3 CheckpointPosition => checkpoint;
        public int CheckpointSequence { get; private set; }
        public string CheckpointHint { get; private set; } = "";
        private bool paused;
        private bool completed;
        private float elapsed;
        private string notice = "";
        private float noticeUntil;

        public bool IsPlaying => !paused && !completed;
        public bool IsPaused => paused;
        public bool IsComplete => completed;
        public float Elapsed => elapsed;
        public string ActiveNotice => Time.unscaledTime < noticeUntil ? notice : "";
        public bool showLegacyHud = true;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            checkpoint = player.transform.position;
            checkpointYaw = player.transform.eulerAngles.y;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (input == null) return;
            if (input.PausePressed && !completed)
            {
                TogglePause();
            }
            if (input.RestartPressed) Restart();
            if (!IsPlaying) return;
            elapsed += Time.deltaTime;
            if (player.transform.position.y < fallHeight) Respawn();
        }

        public void SetCheckpoint(Vector3 position, float facingYaw = 0f, string hint = "", Transform bridgeLessonExit = null)
        {
            // Current slice checkpoints are shared roofs: preserve world, reset facing.
            checkpoint = position;
            checkpointYaw = facingYaw;
            CheckpointHint = hint;
            shiftBridgeLessonExit = bridgeLessonExit;
            if (bridgeLessonExit != null)
            {
                Collider landing = bridgeLessonExit.GetComponent<Collider>();
                shiftBridgeLessonEnd = landing != null ? landing.ClosestPoint(position) : bridgeLessonExit.position;
            }
            CheckpointSequence++;
            feedback?.Checkpoint();
            Notify("CHECKPOINT");
        }

        public void Respawn()
        {
            player.Teleport(checkpoint);
            player.transform.rotation = Quaternion.Euler(0f, checkpointYaw, 0f);
            if (player.visual != null) player.visual.rotation = player.transform.rotation;
            cameraRig?.Recover(checkpointYaw);
            Notify("TRY AGAIN");
        }

        public void Complete()
        {
            if (completed) return;
            completed = true;
            player.StopMotion();
            // Finish may arrive directly from a jump. Reframe from the stopped
            // focus before freezing follow, rather than keeping an airborne offset.
            cameraRig?.Snap();
            feedback?.Goal();
            Notify("ROOFTOP CLEARED");
        }

        public void Notify(string message)
        {
            notice = message;
            noticeUntil = Time.unscaledTime + 1.7f;
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void TogglePause()
        {
            if (completed) return;
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        private void OnGUI()
        {
            if (!showLegacyHud) return;
            int size = Mathf.Max(16, Screen.height / 45);
            GUI.skin.label.fontSize = size;
            GUI.skin.box.fontSize = size;
            GUI.color = Color.white;
            GUI.Box(new Rect(16f, 14f, 290f, 105f), "SHIFTBOUND");
            GUI.Label(new Rect(30f, 47f, 270f, 30f),
                worlds != null && worlds.IsAltered ? "WORLD: OVERGROWN" : "WORLD: PRESENT");
            GUI.Label(new Rect(30f, 75f, 270f, 30f), "TIME " + elapsed.ToString("0.0") + "s");
            GUI.Box(new Rect(16f, Screen.height - 92f, 450f, 76f),
                "WASD / left stick: move   Space / A: jump\nShift / X: switch   Right mouse / right stick: look");
            if (Time.unscaledTime < noticeUntil)
                GUI.Box(new Rect(Screen.width / 2f - 150f, 35f, 300f, 48f), notice);
            if (paused || completed)
            {
                float width = 420f;
                GUI.Box(new Rect((Screen.width - width) / 2f, Screen.height / 2f - 80f,
                    width, 160f), completed
                        ? "LEVEL COMPLETE\nTime: " + elapsed.ToString("0.0") + "s\nR / Select: retry"
                        : "PAUSED\nEscape / Start: resume\nR / Select: restart");
            }
        }
    }
}


