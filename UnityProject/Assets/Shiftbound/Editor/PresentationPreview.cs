using UnityEditor;
using UnityEngine;

// Android remains the build target without silently making desktop Play Mode
// use the phone's shadow/AA profile. Phone preview is an explicit authoring mode.
[InitializeOnLoad]
public static class PresentationPreview
{
    const string Preference = "Shiftbound.Editor.SamsungRenderingPreview";
    const string Menu = "Shiftbound/Preview/Samsung Rendering Profile";
    static PresentationPreview()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            string wanted = EditorPrefs.GetBool(Preference, false) ? "Mobile" : "PC";
            int index = System.Array.IndexOf(QualitySettings.names, wanted);
            if (index >= 0) QualitySettings.SetQualityLevel(index, true);
            Debug.Log("SHIFTBOUND EDITOR PRESENTATION: " + wanted + "; Samsung preview is rendering only, not device performance evidence.");
        };
    }
    [MenuItem(Menu)]
    static void Toggle() => EditorPrefs.SetBool(Preference, !EditorPrefs.GetBool(Preference, false));
    [MenuItem(Menu, true)]
    static bool Validate() { UnityEditor.Menu.SetChecked(Menu, EditorPrefs.GetBool(Preference, false)); return !EditorApplication.isPlayingOrWillChangePlaymode; }
}
