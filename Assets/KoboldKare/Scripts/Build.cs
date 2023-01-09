using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor.Build.Reporting;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
//using UnityEngine.AddressableAssets/KoboldKare.Initialization;
//using UnityEngine.Localization;

public class Build {
    static readonly string[] scenes = {"Assets/KoboldKare/Scenes/MainMenu.unity", "Assets/KoboldKare/Scenes/MainMap.unity", "Assets/KoboldKare/Scenes/ErrorScene.unity" };
    private static string outputDirectory {
        get {
            string dir = Environment.GetEnvironmentVariable("BUILD_DIR");
            if (dir == null) {
                throw new UnityException("Tried to build game without specifying build directory!");
            }
            return string.Format("{0}{1}", dir.TrimEnd(Path.DirectorySeparatorChar), Path.DirectorySeparatorChar);
        }
    }
    [MenuItem("KoboldKare/BuildLinux")]
    static void BuildLinux() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneLinux64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        Environment.SetEnvironmentVariable("BUILD_SUCCESS", report.summary.result == BuildResult.Succeeded ? "1" : "0");
    }

    [MenuItem("KoboldKare/BuildMac")]
    static void BuildMac() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        EditorUserBuildSettings.SetPlatformSettings(
            "Standalone",
            "OSXUniversal",
            "Architecture",
            "x64ARM64" // Possible values: "x64" "ARM64" "x64ARM64"
        );
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.app";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneOSX, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        Environment.SetEnvironmentVariable("BUILD_SUCCESS", report.summary.result == BuildResult.Succeeded ? "1" : "0");
    }

    [MenuItem("KoboldKare/BuildWindows")]
    static void BuildWindows() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.exe";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        Environment.SetEnvironmentVariable("BUILD_SUCCESS", report.summary.result == BuildResult.Succeeded ? "1" : "0");
    }

    [MenuItem("KoboldKare/BuildWindows32")]
    static void BuildWindows32() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.exe";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        Environment.SetEnvironmentVariable("BUILD_SUCCESS", report.summary.result == BuildResult.Succeeded ? "1" : "0");
    }
    private static void GetBuildVersion() {
        string version = Environment.GetEnvironmentVariable("BUILD_NUMBER"); 
        string gitcommit = Environment.GetEnvironmentVariable("GIT_COMMIT")?.Substring(0,8); 
        if (!String.IsNullOrEmpty(version) && !String.IsNullOrEmpty(gitcommit)) {
            PlayerSettings.bundleVersion = $"{version}_{gitcommit}";
        } else if (!String.IsNullOrEmpty(version)) {
            PlayerSettings.bundleVersion = version;
        }
    }
}

#endif
