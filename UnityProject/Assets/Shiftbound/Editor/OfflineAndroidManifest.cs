using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;

// This slice has no online features. Enforce that decision in the final merged
// package rather than making privacy declarations from C# inspection alone.
public sealed class OfflineAndroidManifest : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 1000;
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string launcher = Path.GetFullPath(Path.Combine(path, "../launcher/src/main/AndroidManifest.xml"));
        XDocument manifest = XDocument.Load(launcher);
        XNamespace android = "http://schemas.android.com/apk/res/android";
        XNamespace tooling = "http://schemas.android.com/tools";
        foreach (XElement permission in manifest.Root.Elements("uses-permission").Where(p => (string)p.Attribute(android + "name") == "android.permission.INTERNET").ToArray()) permission.Remove();
        manifest.Root.Add(new XElement("uses-permission", new XAttribute(android+"name","android.permission.INTERNET"), new XAttribute(tooling+"node","remove")));
        manifest.Save(launcher);
    }
}
