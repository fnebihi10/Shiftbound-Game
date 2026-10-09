using UnityEngine;

namespace Shiftbound
{
    public sealed class PremiumHUD : MonoBehaviour
    {
        public GameFlow flow;
        public WorldSwitcher worlds;
        private GUIStyle pill;
        private GUIStyle title;
        private GUIStyle sub;
        private GUIStyle timer;
        private GUIStyle controls;
        private GUIStyle center;
        private GUIStyle lessonTitle;
        private GUIStyle lessonDetail;
        private Texture2D pillTexture;
        private Texture2D darkTexture;
        private bool gamepadPrompts;

        public static string FormatTime(float seconds)
        {
            int totalTenths = Mathf.Max(0, Mathf.FloorToInt(seconds * 10f));
            return string.Format("{0:00}:{1:00}.{2}", totalTenths / 600,
                (totalTenths / 10) % 60, totalTenths % 10);
        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Gamepad.current != null &&
                UnityEngine.InputSystem.Gamepad.current.wasUpdatedThisFrame) gamepadPrompts = true;
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.wasUpdatedThisFrame) gamepadPrompts = false;
        }

        private void EnsureStyles()
        {
            if (pill != null) return;
            pillTexture = Rounded(new Color(0.035f, 0.085f, 0.12f, 0.87f), 48, 14);
            darkTexture = Rounded(new Color(0.02f, 0.04f, 0.06f, 0.92f), 48, 14);
            pill = new GUIStyle(GUI.skin.box)
            {
                normal = { background = pillTexture },
                border = new RectOffset(14, 14, 14, 14),
                padding = new RectOffset(14, 14, 6, 6)
            };
            title = Label(22, FontStyle.Bold, Color.white);
            sub = Label(13, FontStyle.Bold, new Color(0.67f, 0.90f, 0.86f));
            timer = Label(23, FontStyle.Bold, Color.white);
            controls = Label(13, FontStyle.Bold, Color.white);
            center = Label(23, FontStyle.Bold, Color.white);
            center.alignment = TextAnchor.MiddleCenter;
            center.wordWrap = false;
            lessonTitle = Label(20, FontStyle.Bold, new Color(0.67f, 0.90f, 0.86f));
            lessonTitle.alignment = TextAnchor.MiddleCenter;
            lessonDetail = Label(16, FontStyle.Bold, Color.white);
            lessonDetail.alignment = TextAnchor.MiddleCenter;
        }

        private static GUIStyle Label(int fontSize, FontStyle fontStyle, Color color)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = fontStyle,
                normal = { textColor = color },
                alignment = TextAnchor.MiddleLeft
            };
        }

        private static Texture2D Rounded(Color color, int size, int radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.DontSave;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int px = Mathf.Clamp(x, radius, size - radius - 1);
                int py = Mathf.Clamp(y, radius, size - radius - 1);
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(px, py));
                var pixel = color;
                pixel.a *= Mathf.Clamp01(radius + 0.5f - d);
                texture.SetPixel(x, y, pixel);
            }
            texture.Apply();
            return texture;
        }

        private void OnGUI()
        {
            if (flow == null || worlds == null) return;
            EnsureStyles();
            bool phone = flow.input != null && flow.input.Phone.Visible;
            if (flow.input != null) return; // Production Canvas owns all shipped views.
            float scale = Screen.safeArea.height / 720f;
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.safeArea.x, Screen.height - Screen.safeArea.yMax, 0f),
                Quaternion.identity, new Vector3(scale, scale, 1f));
            float w = Screen.safeArea.width / scale;
            float h = 720f;
            const float pad = 20f;

            GUI.Box(new Rect(pad, pad, 250f, 68f), GUIContent.none, pill);
            GUI.Label(new Rect(pad + 18f, pad + 7f, 220f, 22f), "SHIFTBOUND  /  WORLD", sub);
            GUI.Label(new Rect(pad + 18f, pad + 27f, 220f, 31f),
                worlds.IsAltered ? "OVERGROWN" : "PRESENT", title);
            GUI.Box(new Rect(w - pad - 147f, pad, 147f, 58f), GUIContent.none, pill);
            GUI.Label(new Rect(w - pad - 130f, pad + 12f, 120f, 34f),
                FormatTime(flow.Elapsed), timer);

            if (!phone) {
            GUI.Box(new Rect(pad, h - pad - 60f, 530f, 60f), GUIContent.none, pill);
            GUI.Label(new Rect(pad + 14f, h - pad - 57f, 505f, 52f),
                gamepadPrompts ? "LEFT STICK  MOVE     A  JUMP     X  SWITCH\nRIGHT STICK  LOOK     START  PAUSE" :
                "WASD  MOVE     SPACE  JUMP     SHIFT  SWITCH\nHOLD RIGHT MOUSE  LOOK     ESC  PAUSE     R  RETRY", controls);
            }
            if (phone && flow.IsPlaying && flow.Elapsed < 7f)
            {
                GUI.Box(new Rect(w / 2f - 245f, 104f, 490f, 42f), GUIContent.none, pill);
                GUI.Label(new Rect(w / 2f - 235f, 109f, 470f, 32f), "Hold JUMP; slide onto SHIFT for midair change", lessonDetail);
            }
            if (flow.HasShiftBridgeGuidance)
            {
                float width = Mathf.Min(580f, w - pad * 2f);
                float left = (w - width) / 2f;
                GUI.Box(new Rect(left, 106f, width, 86f), GUIContent.none, pill);
                GUI.Label(new Rect(left + 10f, 112f, width - 20f, 30f),
                    worlds.IsAltered ? "BRIDGE SOLID — CROSS IN OVERGROWN" :
                    phone ? "TAP SHIFT TO MAKE THE BRIDGE SOLID" : gamepadPrompts ? "PRESS X TO MAKE THE BRIDGE SOLID" :
                    "PRESS SHIFT TO MAKE THE BRIDGE SOLID", lessonTitle);
                GUI.Label(new Rect(left + 10f, 145f, width - 20f, 37f),
                    worlds.IsAltered
                        ? (phone ? "Move forward and hold JUMP to jump across." : gamepadPrompts ? "Move forward and hold A to jump across." :
                            "Move forward and hold SPACE to jump across.")
                        : "Blue previews cannot support you. Switch before jumping.", lessonDetail);
            }
            else if (flow.IsPlaying && !string.IsNullOrEmpty(flow.CheckpointHint))
            {
                float hintY = phone ? 104f : h - pad - 130f;
                GUI.Box(new Rect(w / 2f - 265f, hintY, 530f, 60f), GUIContent.none, pill);
                string hint = flow.CheckpointHint;
                if (phone) hint = hint.Replace("SPACE", "JUMP").Replace("Space", "JUMP").Replace("press Shift", "tap SHIFT");
                GUI.Label(new Rect(w / 2f - 251f, hintY + 4f, 505f, 52f), hint, controls);
            }

            if (!string.IsNullOrEmpty(flow.ActiveNotice))
            {
                GUI.Box(new Rect(w / 2f - 210f, 202f, 420f, 48f),
                    GUIContent.none, pill);
                GUI.Label(new Rect(w / 2f - 200f, 207f, 400f, 38f),
                    flow.ActiveNotice, center);
            }
            if (false && !phone && (flow.IsPaused || flow.IsComplete))
            {
                GUI.Box(new Rect(w / 2f - 240f, 180f, 480f, 370f),
                    GUIContent.none, new GUIStyle(pill) { normal = { background = darkTexture } });
                GUI.Label(new Rect(w / 2f - 215f, 194f, 430f, 80f),
                    flow.IsComplete
                        ? "ROOFTOP CLEARED\n" + FormatTime(flow.Elapsed) + "\nRetry below"
                        : "PAUSED\nResume or restart below",
                    center);
                if (flow.IsPaused && GUI.Button(new Rect(w / 2f - 210f, 283f, 195f, 40f), "RESUME"))
                    flow.TogglePause();
                if (GUI.Button(new Rect(w / 2f + 15f, 283f, 195f, 40f), "RETRY"))
                    flow.Restart();
                if (Event.current.type == EventType.MouseDown) GUI.FocusControl(null);
                Setting(w, 338f, "Mouse sensitivity", ref PlayerPreferences.MouseSensitivity, .3f, 2.5f);
                Setting(w, 381f, "Controller sensitivity", ref PlayerPreferences.StickSensitivity, .3f, 2.5f);
                Setting(w, 424f, "Volume", ref PlayerPreferences.Volume, 0f, 1f);
                AudioListener.volume = PlayerPreferences.Volume;
                bool inverted = GUI.Toggle(new Rect(w/2f - 210f, 480f, 420f, 32f), PlayerPreferences.InvertY, "Invert vertical camera");
                if (inverted != PlayerPreferences.InvertY) { PlayerPreferences.InvertY = inverted; PlayerPreferences.Save(); }
            }
            GUI.matrix = previous;
        }

        private void Setting(float width, float y, string text, ref float value, float min, float max)
        {
            GUI.Label(new Rect(width/2f - 210f, y, 220f, 30f), text, controls);
            float next = GUI.HorizontalSlider(new Rect(width/2f + 20f, y+9f, 185f, 25f), value, min, max);
            if (!Mathf.Approximately(next, value))
            {
                value = next;
                PlayerPreferences.Save();
            }
        }

        private void OnDestroy()
        {
            if (pillTexture != null) Destroy(pillTexture);
            if (darkTexture != null) Destroy(darkTexture);
        }
    }
}

