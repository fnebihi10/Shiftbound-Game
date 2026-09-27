using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shiftbound
{
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
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            if (input.PausePressed && !completed)
            {
                paused = !paused;
                Time.timeScale = paused ? 0f : 1f;
            }
            if (input.RestartPressed) Restart();
            if (!IsPlaying) return;
            elapsed += Time.deltaTime;
            if (player.transform.position.y < fallHeight) Respawn();
        }

        public void SetCheckpoint(Vector3 position)
        {
            checkpoint = position;
            feedback?.Checkpoint();
            Notify("CHECKPOINT");
        }

        public void Respawn()
        {
            player.Teleport(checkpoint);
            cameraRig?.Snap();
            Notify("TRY AGAIN");
        }

        public void Complete()
        {
            if (completed) return;
            completed = true;
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


