using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
//using UnityEngine.AddressableAssets/KoboldKare.Initialization;
//using UnityEngine.Localization;

public class Build {
    
    static readonly string[] scenes = {"Assets/KoboldKare/Scenes/AddressableLoadingScene.unity"};
    private static string outputDirectory {
        get {
            string dir = Environment.GetEnvironmentVariable("BUILD_DIR");
            if (dir == null) {
                throw new UnityException("Tried to build game without specifying build directory!");
            }
            return string.Format("{0}{1}", dir.TrimEnd(Path.DirectorySeparatorChar), Path.DirectorySeparatorChar);
        }
    }

    private static int ResultToExitCode(BuildResult result) {
        switch (result) {
            case BuildResult.Succeeded: return 0;
            case BuildResult.Failed: return 1; 
            case BuildResult.Unknown: return 2;
            case BuildResult.Cancelled: return 3;
            default: return 4;
        }
    }

    static void BuildLinux() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,BuildTarget.StandaloneLinux64);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneLinux64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        EditorApplication.Exit(ResultToExitCode(report.summary.result));
    }

    static void BuildMac() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        EditorUserBuildSettings.SetPlatformSettings(
            "Standalone",
            "OSXUniversal",
            "Architecture",
            "x64ARM64" // Possible values: "x64" "ARM64" "x64ARM64"
        );
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,BuildTarget.StandaloneOSX);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.app";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneOSX, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        EditorApplication.Exit(ResultToExitCode(report.summary.result));
    }

    static void BuildWindows() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows64);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.exe";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        EditorApplication.Exit(ResultToExitCode(report.summary.result));
    }

    static void BuildWindows32() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,BuildTarget.StandaloneWindows);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.exe";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        EditorApplication.Exit(ResultToExitCode(report.summary.result));
    }
    private static void GetBuildVersion() {
        string version = Environment.GetEnvironmentVariable("BUILD_NUMBER");
        string date = DateTime.Now.ToString("MM.dd.yyyy");
        string gitcommit = Environment.GetEnvironmentVariable("GIT_COMMIT")?.Substring(0,8); 
        if (!String.IsNullOrEmpty(version) && !String.IsNullOrEmpty(gitcommit)) {
            PlayerSettings.bundleVersion = $"{date}_{gitcommit}";
        } else if (!String.IsNullOrEmpty(version)) {
            PlayerSettings.bundleVersion = version;
        }
    }
}

#endif
