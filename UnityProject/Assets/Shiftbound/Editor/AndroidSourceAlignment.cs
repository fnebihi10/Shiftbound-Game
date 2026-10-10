using System.IO;
using UnityEditor.Android;

// Only source-built Gradle C++ targets receive these linker options. Unity's
// libmain/libunity and the NDK C++ shared runtime are prebuilts, still inspected
// independently; these options make no claim to repair those vendor binaries.
public sealed class AndroidSourceAlignment : IPostGenerateGradleAndroidProject
{
    public int callbackOrder=>1100;
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string cmake=Path.Combine(path,"src/main/cpp/CMakeLists.txt");
        const string marker="# Shiftbound source-target page alignment";
        string content=File.ReadAllText(cmake);
        if(content.Contains(marker))return;
        File.AppendAllText(cmake,"\n"+marker+"\nforeach(sb_target game swappywrapper)\n  if(TARGET ${sb_target})\n    target_link_options(${sb_target} PRIVATE \"-Wl,-z,max-page-size=16384\" \"-Wl,-z,common-page-size=16384\")\n  endif()\nendforeach()\n");
    }
}
