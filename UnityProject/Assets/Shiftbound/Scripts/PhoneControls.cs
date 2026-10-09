using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace Shiftbound
{
    [DefaultExecutionOrder(-400)]
    public sealed class PhoneControls : MonoBehaviour
    {
        public readonly TouchInputRouter Router = new TouchInputRouter();
        public bool Visible => Application.isMobilePlatform || Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundTouchUI") >= 0;
        public float Scale => Screen.safeArea.height / 720f;
        public float Width => Screen.safeArea.width / Scale;
        public float Height => 720f;
        public Rect PauseRect => new Rect(Width - 250f, 18f, 64f, 60f);
        private bool suppressed;
        private GUIStyle label;
        private Texture2D disc;
        private int diagnosticTaps;
        private int menuPointer = -1;
        private int sliderRow = -1;
        public double LastEventTime { get; private set; }
        public int EventFrame { get; private set; } = -1;

        private void OnEnable() { EnhancedTouchSupport.Enable(); }
        private void Start() { gameObject.AddComponent<ProductionHUDCanvas>(); }
        private void OnDisable() { Router.Reset(); EnhancedTouchSupport.Disable(); }
        public void Cancel() { Router.Reset(); suppressed = true; menuPointer = -1; sliderRow = -1; PlayerPreferences.Save(); }
        public Vector2 ToUI(Vector2 screen) => new Vector2((screen.x - Screen.safeArea.x) / Scale,
            (Screen.safeArea.yMax - screen.y) / Scale);
        public void Layout()
        {
            float s = PlayerPreferences.ControlScale;
            float inset = PlayerPreferences.ControlInset;
            float bottom = Height - 36f - PlayerPreferences.ControlHeight;
            Router.Stick = new Rect(34f + inset, bottom - 192f * s, 192f * s, 192f * s);
            Router.Jump = new Rect(Width - 40f - inset - 124f * s, bottom - 124f * s, 124f * s, 124f * s);
            Router.Shift = new Rect(Router.Jump.x - 82f * s - 12f, bottom - 160f * s, 92f * s, 92f * s);
            Router.Camera = new Rect(Width * .43f, 100f, Width * .57f, Height - 100f);
        }
        private void Update()
        {
            if (!Visible) { DesktopMenu(); return; }
            Layout(); Router.BeginFrame();
            var contacts = Touch.activeTouches;
            if (suppressed)
            {
                if (contacts.Count == 0) suppressed = false;
                return;
            }
            foreach (var touch in contacts)
            {
                Vector2 p = ToUI(touch.screenPosition);
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                { LastEventTime = touch.time; EventFrame = Time.frameCount; }
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) {
                        if(menuPointer<0){menuPointer=touch.touchId;MenuTouch(p,true);} continue;
                    }
                    if (PauseRect.Contains(p)) { GameFlow.Instance?.TogglePause(); Cancel(); break; }
                    if (GameFlow.Instance == null || GameFlow.Instance.IsPlaying) Router.Begin(touch.touchId, p);
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    Router.End(touch.touchId);
                    if(menuPointer==touch.touchId){menuPointer=-1;sliderRow=-1;PlayerPreferences.Save();}
                }
                else if (GameFlow.Instance != null && !GameFlow.Instance.IsPlaying) {if(menuPointer==touch.touchId)MenuTouch(p,false);}
                else Router.Drag(touch.touchId, p);
            }
        }
        private void DesktopMenu()
        {
            var mouse=UnityEngine.InputSystem.Mouse.current;
            if(mouse==null || GameFlow.Instance==null || GameFlow.Instance.IsPlaying)return;
            Vector2 p=ToUI(mouse.position.ReadValue());
            if(mouse.leftButton.wasPressedThisFrame)MenuTouch(p,true);
            else if(mouse.leftButton.isPressed)MenuTouch(p,false);
            if(mouse.leftButton.wasReleasedThisFrame){sliderRow=-1;PlayerPreferences.Save();}
        }
        private void LegacyMenu()
        {
            if (!Visible || GameFlow.Instance == null) return;
            if (GameFlow.Instance.IsPlaying) return; // Canvas owns the play HUD.
            Layout();
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.safeArea.x, Screen.height - Screen.safeArea.yMax, 0f),
                Quaternion.identity, new Vector3(Scale, Scale, 1f));
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                disc = new Texture2D(64, 64, TextureFormat.RGBA32, false);
                for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                    disc.SetPixel(x, y, new Color(.04f, .09f, .12f, Mathf.Clamp01(32f - d) * .62f));
                }
                disc.Apply();
            }
            if (GameFlow.Instance.IsPlaying)
            {
                Draw(Router.Stick, "MOVE"); Draw(Router.Jump, Router.JumpHeld ? "HOLD" : "JUMP"); Draw(Router.Shift, "SHIFT");
                Vector2 thumb = Router.Stick.center + new Vector2(Router.Move.x, -Router.Move.y) * Router.Stick.width * .25f;
                GUI.color = new Color(.65f, .9f, .87f, .9f);
                GUI.DrawTexture(new Rect(thumb.x - 22f, thumb.y - 22f, 44f, 44f), disc);
                GUI.color = Color.white;
                GUI.Box(PauseRect, "II");
            }
            else DrawMenu();
            GUI.matrix = old;
        }
        public Rect MenuRect(int row) => new Rect(Width * .5f - 250f, 126f + row * 51f, 500f, 46f);
        private void MenuTouch(Vector2 p, bool began)
        {
            if(began)sliderRow=-1;
            if(GameFlow.Instance.IsComplete){if(began&&MenuRect(1).Contains(p)){GameFlow.Instance.Restart();Cancel();}return;}
            if (began && new Rect(Width*.5f-250f,80f,500f,42f).Contains(p)) diagnosticTaps++;
            if (began && MenuRect(0).Contains(p)) { GameFlow.Instance.TogglePause(); Cancel(); return; }
            if (began && MenuRect(1).Contains(p)) { GameFlow.Instance.Restart(); Cancel(); return; }
            for (int row = 2; row <= 7; row++)
            {
                Rect r = MenuRect(row);
                if (row==7) { if(began&&r.Contains(p)){PlayerPreferences.CameraAssist=!PlayerPreferences.CameraAssist;PlayerPreferences.Save();} continue; }
                // Only a begin in the slider track acquires it. Text taps and
                // stationary contacts elsewhere never mutate or save preferences.
                if (began && r.Contains(p) && p.x>=r.x+210f) sliderRow=row;
                if(sliderRow!=row)continue;
                float t = Mathf.Clamp01((p.x - r.x - 210f) / 275f);
                if (row == 2) PlayerPreferences.ControlScale = Mathf.Lerp(.8f, 1.35f, t);
                if (row == 3) PlayerPreferences.ControlInset = t * 90f;
                if (row == 4) PlayerPreferences.ControlHeight = t * 100f;
                if (row == 5) PlayerPreferences.TouchSensitivity = Mathf.Lerp(.3f, 2.5f, t);
                if (row == 6) { PlayerPreferences.Volume = t; AudioListener.volume = t; }
            }
            if (began && MenuRect(8).Contains(p)) PlayerPreferences.InvertY = !PlayerPreferences.InvertY;
            if (began && MenuRect(9).Contains(p)) { PlayerPreferences.LowPower = !PlayerPreferences.LowPower; GameFlow.Instance.ApplyFrameTier(); }
            if (ShowDiagnostics && began && MenuRect(10).Contains(p))
            {
                var telemetry = FindFirstObjectByType<GameplayTelemetry>();
                if (telemetry == null) telemetry = new GameObject("QA telemetry").AddComponent<GameplayTelemetry>();
                if (telemetry.Collecting) telemetry.End(); else telemetry.Begin();
            }
            if(began && (MenuRect(8).Contains(p)||MenuRect(9).Contains(p)))PlayerPreferences.Save();
        }
        public bool ShowDiagnostics => diagnosticTaps >= 5 || Debug.isDebugBuild || Array.IndexOf(Environment.GetCommandLineArgs(), "-shiftboundQA") >= 0;
        private void DrawMenu()
        {
            GUI.Box(new Rect(Width * .5f - 270f, 78f, 540f, ShowDiagnostics ? 615f : 565f), GUIContent.none);
            GUI.Label(new Rect(Width * .5f - 250f, 80f, 500f, 42f), GameFlow.Instance.IsComplete ? "ROOFTOP CLEARED" : "PAUSED", label);
            DrawMenuRow(0, GameFlow.Instance.IsComplete ? "Time " + PremiumHUD.FormatTime(GameFlow.Instance.Elapsed) : "RESUME", -1f);
            DrawMenuRow(1, "RESTART FROM BEGINNING", -1f);
            DrawMenuRow(2, "Control size", Mathf.InverseLerp(.8f, 1.35f, PlayerPreferences.ControlScale));
            DrawMenuRow(3, "Horizontal inset", PlayerPreferences.ControlInset / 90f);
            DrawMenuRow(4, "Control height", PlayerPreferences.ControlHeight / 100f);
            DrawMenuRow(5, "Touch sensitivity", Mathf.InverseLerp(.3f, 2.5f, PlayerPreferences.TouchSensitivity));
            DrawMenuRow(6, "Volume", PlayerPreferences.Volume);
            DrawMenuRow(7, "Camera follow: " + (PlayerPreferences.CameraAssist ? "ON" : "OFF"), -1f);
            DrawMenuRow(8, "Invert vertical: " + (PlayerPreferences.InvertY ? "ON" : "OFF"), -1f);
            DrawMenuRow(9, PlayerPreferences.LowPower ? "Frame cap: 30 FPS" : "Frame cap: 60 FPS", -1f);
            if (ShowDiagnostics) DrawMenuRow(10, "QA: start / stop frame recording", -1f);
        }
        private void DrawMenuRow(int row, string text, float value)
        {
            Rect r = MenuRect(row); GUI.Box(r, GUIContent.none);
            GUI.Label(new Rect(r.x + 8f, r.y, value < 0f ? r.width - 16f : 200f, r.height), text, label);
            if (value >= 0f)
            {
                GUI.color = new Color(.4f, .85f, .75f);
                GUI.DrawTexture(new Rect(r.x + 210f, r.y + 18f, Mathf.Max(8f, 275f * value), 10f), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }
        }
        private void Draw(Rect rect, string text) { GUI.DrawTexture(rect, disc); GUI.Label(rect, text, label); }
        private void OnDestroy() { if (disc != null) Destroy(disc); }
    }
}
