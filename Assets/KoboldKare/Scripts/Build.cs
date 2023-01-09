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
    
    static readonly string[] scenes = {"Assets/KoboldKare/Scenes/MainMenu.unity", "Assets/KoboldKare/Scenes/MainMap.unity", "Assets/KoboldKare/Scenes/ErrorScene.unity" };
    private const string buildStatusVariable = "BUILD_RESULT";
    private static string outputDirectory {
        get {
            string dir = Environment.GetEnvironmentVariable("BUILD_DIR");
            if (dir == null) {
                throw new UnityException("Tried to build game without specifying build directory!");
            }
            return string.Format("{0}{1}", dir.TrimEnd(Path.DirectorySeparatorChar), Path.DirectorySeparatorChar);
        }
    }
    
    private static void SetEnvironmentVariable(string name, string value) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            System.Diagnostics.Process.Start("/bin/bash", $"-c export {name} = {value}");
        } else {
            Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
        }
    }

    private static string ResultToString(BuildResult result) {
        switch (result) {
            case BuildResult.Unknown: return "Unknown";
            case BuildResult.Succeeded: return "Succeeded";
            case BuildResult.Cancelled: return "Cancelled";
            case BuildResult.Failed: return "Failed"; 
            default: return result.ToString();
        }
    }

    static void BuildLinux() {
        SetEnvironmentVariable(buildStatusVariable, ResultToString(BuildResult.Unknown));
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneLinux64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        SetEnvironmentVariable(buildStatusVariable,  ResultToString(report.summary.result));
    }

    static void BuildMac() {
        SetEnvironmentVariable(buildStatusVariable, ResultToString(BuildResult.Unknown));
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
        SetEnvironmentVariable(buildStatusVariable,  ResultToString(report.summary.result));
    }

    static void BuildWindows() {
        SetEnvironmentVariable(buildStatusVariable, ResultToString(BuildResult.Unknown));
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.exe";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        SetEnvironmentVariable(buildStatusVariable,  ResultToString(report.summary.result));
    }

    static void BuildWindows32() {
        SetEnvironmentVariable(buildStatusVariable, ResultToString(BuildResult.Unknown));
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "false");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = $"{outputDirectory}KoboldKare.exe";
        Debug.Log($"#### BUILDING TO {output}####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        SetEnvironmentVariable(buildStatusVariable,  ResultToString(report.summary.result));
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
