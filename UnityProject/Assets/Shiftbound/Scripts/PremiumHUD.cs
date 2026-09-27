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
        private Texture2D pillTexture;
        private Texture2D darkTexture;

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
            float scale = Mathf.Clamp(Screen.height / 720f, 0.72f, 1.45f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float w = Screen.width / scale;
            float h = Screen.height / scale;
            const float pad = 20f;

            GUI.Box(new Rect(pad, pad, 250f, 68f), GUIContent.none, pill);
            GUI.Label(new Rect(pad + 18f, pad + 7f, 220f, 22f), "SHIFTBOUND  /  WORLD", sub);
            GUI.Label(new Rect(pad + 18f, pad + 27f, 220f, 31f),
                worlds.IsAltered ? "OVERGROWN" : "PRESENT", title);
            GUI.Box(new Rect(w - pad - 147f, pad, 147f, 58f), GUIContent.none, pill);
            GUI.Label(new Rect(w - pad - 130f, pad + 12f, 120f, 34f),
                flow.Elapsed.ToString("00:00.0"), timer);

            GUI.Box(new Rect(pad, h - pad - 42f, 430f, 42f), GUIContent.none, pill);
            GUI.Label(new Rect(pad + 14f, h - pad - 39f, 410f, 34f),
                "WASD  MOVE     SPACE  JUMP     SHIFT  SWITCH", controls);

            if (!string.IsNullOrEmpty(flow.ActiveNotice))
            {
                GUI.Box(new Rect(w / 2f - 130f, pad, 260f, 48f),
                    GUIContent.none, pill);
                GUI.Label(new Rect(w / 2f - 120f, pad + 5f, 240f, 38f),
                    flow.ActiveNotice, center);
            }
            if (flow.IsPaused || flow.IsComplete)
            {
                GUI.Box(new Rect(w / 2f - 210f, h / 2f - 90f, 420f, 180f),
                    GUIContent.none, new GUIStyle(pill) { normal = { background = darkTexture } });
                GUI.Label(new Rect(w / 2f - 185f, h / 2f - 55f, 370f, 100f),
                    flow.IsComplete
                        ? "ROOFTOP CLEARED\n" + flow.Elapsed.ToString("0.0") + " seconds\nR / Select to retry"
                        : "PAUSED\nEscape / Start to resume\nR / Select to restart",
                    center);
            }
            GUI.matrix = previous;
        }

        private void OnDestroy()
        {
            if (pillTexture != null) Destroy(pillTexture);
            if (darkTexture != null) Destroy(darkTexture);
        }
    }
}

