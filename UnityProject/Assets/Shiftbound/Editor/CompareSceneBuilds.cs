using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CompareSceneBuilds
{
    public static void BuildBoth()
    {
        foreach (string sceneName in new[] { "SkylineRooftops", "GoldenRooftops" })
        {
            string output = Path.GetFullPath(Path.Combine(Application.dataPath,
                "../Builds/Comparison" + sceneName + "/Shiftbound.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Shiftbound/Scenes/" + sceneName + ".unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception(sceneName + " build failed: " + report.summary.result);
            Debug.Log("SHIFTBOUND COMPARISON BUILD PASSED: " + sceneName + " " + output);
        }
    }
}
