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
        private bool DiagnosticSession => System.Array.Exists(System.Environment.GetCommandLineArgs(),
            arg => arg.StartsWith("-shiftbound", System.StringComparison.Ordinal) && arg != "-shiftboundTouchUI");

        private void Awake()
        {
            Instance = this;
            if (DiagnosticSession) Application.runInBackground = true;
            Time.timeScale = 1f;
            AudioListener.pause = false;
            checkpoint = player.transform.position;
            checkpointYaw = player.transform.eulerAngles.y;
        }

        private void Start()
        {
            // Store authored checkpoint identity, never arbitrary air positions.
            string saved = DiagnosticSession ? "" : PlayerPrefs.GetString("sb.checkpoint", "");
            foreach (StageTrigger trigger in FindObjectsByType<StageTrigger>(FindObjectsSortMode.None))
            {
                if (trigger.name != saved || trigger.kind != StageTrigger.TriggerKind.Checkpoint || !trigger.hasCheckpointPosition) continue;
                checkpoint = trigger.checkpointPosition;
                checkpointYaw = trigger.checkpointFacingYaw;
                CheckpointHint = trigger.checkpointHint;
                shiftBridgeLessonExit = trigger.shiftBridgeLessonExit;
                if (shiftBridgeLessonExit != null)
                {
                    Collider landing = shiftBridgeLessonExit.GetComponent<Collider>();
                    shiftBridgeLessonEnd = landing != null ? landing.ClosestPoint(checkpoint) : shiftBridgeLessonExit.position;
                }
                Respawn();
                break;
            }
            AudioListener.volume = PlayerPreferences.Volume;
            ApplyFrameTier();
        }

        public void ApplyFrameTier()
        {
            if (Application.isMobilePlatform) Application.targetFrameRate = PlayerPreferences.LowPower ? 30 : 60;
        }

        private void OnApplicationPause(bool interrupted) { if (interrupted && !DiagnosticSession) Suspend(); }
        private void OnApplicationFocus(bool focused) { if (!focused && !DiagnosticSession) Suspend(); }
        public void Suspend()
        {
            if (!completed && !paused) TogglePause();
            input?.CancelGameplayInput();
            player?.ClearJumpIntent();
            PlayerPreferences.Save();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
            AudioListener.pause = false;
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
            if (!DiagnosticSession) foreach (StageTrigger trigger in FindObjectsByType<StageTrigger>(FindObjectsSortMode.None))
                if (trigger.kind == StageTrigger.TriggerKind.Checkpoint && trigger.hasCheckpointPosition &&
                    Vector3.Distance(trigger.checkpointPosition, position) < .01f)
                { PlayerPrefs.SetString("sb.checkpoint", trigger.name); PlayerPrefs.Save(); break; }
            feedback?.Checkpoint();
            Notify("CHECKPOINT");
        }

        public void Respawn()
        {
            input?.CancelGameplayInput();
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
            if (!DiagnosticSession) { PlayerPrefs.DeleteKey("sb.checkpoint"); PlayerPrefs.Save(); }
            input?.CancelGameplayInput();
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
            PlayerPrefs.DeleteKey("sb.checkpoint"); PlayerPrefs.Save();
            input?.CancelGameplayInput();
            AudioListener.pause = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void TogglePause()
        {
            if (completed) return;
            paused = !paused;
            input?.CancelGameplayInput();
            player?.ClearJumpIntent();
            AudioListener.pause = paused;
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


