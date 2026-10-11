using UnityEngine;

namespace Shiftbound
{
    // Preserve the preferred desktop scene/colour/animation. Bound only Android
    // framebuffer cost; do not compound this with a second URP downscale.
    public static class AndroidDisplayBudget
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Configure()
        {
            if (Application.platform != RuntimePlatform.Android) return;
            int width = Screen.width, height = Screen.height;
            int longest = Mathf.Max(width, height);
            if (longest > 1920)
            {
                float scale = 1920f / longest;
                width = Mathf.RoundToInt(width * scale / 2f) * 2;
                height = Mathf.RoundToInt(height * scale / 2f) * 2;
                Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
            }
            Debug.Log($"SHIFTBOUND WINDOWS-POLISHED ANDROID: requested={width}x{height}; URP scale1/MSAA2; original scene/colour/rig/motion; target60 is unverified");
        }
    }
}
