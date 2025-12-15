#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildCleanup_DoNotShipFolders : IPostprocessBuildWithReport
{
    public int callbackOrder => 999;

    public void OnPostprocessBuild(BuildReport report)
    {
        var outputPath = report.summary.outputPath;
        var dir = GetBuildDirectory(outputPath);
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;

        string[] patterns =
        {
            "*_BurstDebugInformation_DoNotShip",
            "*_BackUpThisFolder_ButDontShipItWithYourGame"
        };

        foreach (var pattern in patterns)
        {
            var folders = Directory.GetDirectories(dir, pattern, SearchOption.TopDirectoryOnly);
            foreach (var f in folders)
            {
                try
                {
                    Directory.Delete(f, true);
                    Debug.Log($"[BuildCleanup] Deleted: {f}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[BuildCleanup] Failed to delete {f}\n{e.Message}");
                }
            }
        }
    }

    static string GetBuildDirectory(string outputPath)
    {
        // Windows: .../Game.exe  -> dir
        // Android: .../Game.apk  -> dir
        // macOS: .../Game.app    -> parent dir
        if (string.IsNullOrEmpty(outputPath)) return null;

        var dir = Path.GetDirectoryName(outputPath);
        return dir;
    }
}
#endif
