using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CandidateProvenance
{
    [Serializable] public sealed class Entry { public string path, sha256; }
    [Serializable] public sealed class Manifest
    {
        public string revision, sourceFingerprint, unity, scene, target, createdUtc, signing;
        public string sourceHashPolicy = "utf8-lf-v1; binary files retain exact SHA256";
        public Entry[] sources, binaries;
    }
    private static string Root => Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
    private static string Hash(byte[] bytes)
    {
        using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
    }
    private static Entry[] Sources()
    {
        var paths = new List<string>();
        foreach (string dir in new[] { "Assets", "Packages", "ProjectSettings" })
            paths.AddRange(Directory.GetFiles(Path.Combine(Root, "UnityProject", dir), "*", SearchOption.AllDirectories));
        // Unity's performance-test package creates/removes these transient build
        // resources. They are tooling output, not candidate gameplay source.
        return paths.Where(p => !p.EndsWith("CandidateIdentity.json", StringComparison.Ordinal) &&
            !p.EndsWith("CandidateIdentity.json.meta", StringComparison.Ordinal) &&
            !p.EndsWith("PerformanceTestRunInfo.json", StringComparison.Ordinal) && !p.EndsWith("PerformanceTestRunInfo.json.meta", StringComparison.Ordinal) &&
            !p.EndsWith("PerformanceTestRunSettings.json", StringComparison.Ordinal) && !p.EndsWith("PerformanceTestRunSettings.json.meta", StringComparison.Ordinal))
            .Select(p => new Entry { path = p.Substring(Root.Length + 1).Replace('\\', '/'), sha256 = SourceHash(p) })
            .OrderBy(e => e.path, StringComparer.Ordinal).ToArray();
    }
    private static string SourceHash(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        string extension = Path.GetExtension(path).ToLowerInvariant();
        // Explicit text formats only. .asset can be binary: Unity YAML is recognized
        // by its header, so mesh/texture payloads never lose byte-integrity checks.
        bool text = new[] { ".cs", ".shader", ".hlsl", ".cginc", ".compute", ".json", ".inputactions", ".shadergraph", ".shadersubgraph", ".uxml", ".uss", ".meta", ".txt", ".md", ".xml", ".asmdef", ".asmref" }.Contains(extension) ||
            ((extension == ".asset" || extension == ".mat" || extension == ".prefab" || extension == ".unity" || extension == ".controller") &&
             bytes.Length >= 5 && Encoding.ASCII.GetString(bytes, 0, 5) == "%YAML");
        if (text) bytes = new UTF8Encoding(false, true).GetBytes(new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF').Replace("\r\n", "\n").Replace("\r", "\n"));
        return Hash(bytes);
    }
    private static string Fingerprint(Entry[] entries) => Hash(Encoding.UTF8.GetBytes(string.Concat(entries.Select(e => e.path + ":" + e.sha256 + "\n"))));
    private static void SaveAuthoringSettings()
    {
        // URP's container populates m_RuntimeSettings only while building and
        // clears it in OnBeforeSerialize outside a build (package source verified).
        // Save its authoring form on both sides so platform-derived runtime lists
        // do not make identical authoring source look different. Hash every byte.
        foreach (string guid in AssetDatabase.FindAssets("t:RenderPipelineGlobalSettings"))
            EditorUtility.SetDirty(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)));
        AssetDatabase.SaveAssets();
    }
    public static Manifest Begin(string target, string signing = "none")
    {
        SaveAuthoringSettings();
        if (!AssetDatabase.IsValidFolder("Assets/Shiftbound/Resources")) AssetDatabase.CreateFolder("Assets/Shiftbound", "Resources");
        string revision = "unavailable";
        using (var git = Process.Start(new ProcessStartInfo("git", "rev-parse HEAD") { WorkingDirectory = Root,
            UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true }))
        { revision = git.StandardOutput.ReadToEnd().Trim(); git.WaitForExit(); }
        var entries = Sources();
        var manifest = new Manifest { revision = revision, sourceFingerprint = Fingerprint(entries), sources = entries,
            unity = Application.unityVersion, scene = "Assets/Shiftbound/Scenes/GoldenRooftops.unity", target = target,
            createdUtc = DateTime.UtcNow.ToString("o"), signing = signing };
        File.WriteAllText("Assets/Shiftbound/Resources/CandidateIdentity.json", JsonUtility.ToJson(manifest, true));
        AssetDatabase.ImportAsset("Assets/Shiftbound/Resources/CandidateIdentity.json", ImportAssetOptions.ForceSynchronousImport);
        return manifest;
    }
    public static void Finish(Manifest manifest, string output)
    {
        SaveAuthoringSettings();
        Entry[] now = Sources();
        string current = Fingerprint(now);
        if (manifest.sourceFingerprint != current)
        {
            var before = manifest.sources.ToDictionary(e => e.path, e => e.sha256);
            string changed = string.Join(", ", now.Where(e => !before.TryGetValue(e.path, out string hash) || hash != e.sha256).Select(e => e.path)
                .Concat(before.Keys.Except(now.Select(e => e.path))));
            throw new Exception("Source changed during build; refuse correspondence certification. Rebuild. Changed: " + changed);
        }
        string dir = Path.GetDirectoryName(output);
        var paths = manifest.target == "Windows" ? Directory.GetFiles(dir, "*", SearchOption.AllDirectories) : new[] { output };
        manifest.binaries = paths.Where(p => !p.EndsWith("manifest.json", StringComparison.Ordinal))
            .Select(p => new Entry { path = p.Substring(dir.Length + 1).Replace('\\', '/'), sha256 = Hash(File.ReadAllBytes(p)) })
            .OrderBy(e => e.path, StringComparer.Ordinal).ToArray();
        File.WriteAllText(output + ".manifest.json", JsonUtility.ToJson(manifest, true));
        UnityEngine.Debug.Log("SHIFTBOUND SOURCE CORRESPONDENCE: " + manifest.sourceFingerprint);
    }
}
