using UnityEngine;

namespace Shiftbound
{
    public static class PlayerPreferences
    {
        public static float TouchSensitivity = Get("touch", 1f, .3f, 2.5f);
        public static float MouseSensitivity = Get("mouse", 1f, .3f, 2.5f);
        public static float StickSensitivity = Get("stick", 1f, .3f, 2.5f);
        public static float ControlScale = Get("size", 1f, .8f, 1.35f);
        public static float ControlInset = Get("inset", 0f, 0f, 90f);
        public static float ControlHeight = Get("height", 0f, 0f, 100f);
        public static float Volume = Get("volume", .8f, 0f, 1f);
        public static bool InvertY = PlayerPrefs.GetInt("sb.invert", 0) != 0;
        public static bool CameraAssist = PlayerPrefs.GetInt("sb.assist", 1) != 0;
        public static bool LowPower = PlayerPrefs.GetInt("sb.lowpower", 0) != 0;
        private static float Get(string key, float value, float min, float max) =>
            Mathf.Clamp(PlayerPrefs.GetFloat("sb." + key, value), min, max);
        public static void Save()
        {
            PlayerPrefs.SetFloat("sb.touch", TouchSensitivity);
            PlayerPrefs.SetFloat("sb.mouse", MouseSensitivity);
            PlayerPrefs.SetFloat("sb.stick", StickSensitivity);
            PlayerPrefs.SetFloat("sb.size", ControlScale);
            PlayerPrefs.SetFloat("sb.inset", ControlInset);
            PlayerPrefs.SetFloat("sb.height", ControlHeight);
            PlayerPrefs.SetFloat("sb.volume", Volume);
            PlayerPrefs.SetInt("sb.invert", InvertY ? 1 : 0);
            PlayerPrefs.SetInt("sb.assist", CameraAssist ? 1 : 0);
            PlayerPrefs.SetInt("sb.lowpower", LowPower ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
