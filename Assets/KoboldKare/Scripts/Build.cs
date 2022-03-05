using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
//using UnityEngine.AddressableAssets/KoboldKare.Initialization;
//using UnityEngine.Localization;

public class Build {
    static string[] scenes = {"Assets/KoboldKare/Scenes/MainMenu.unity", "Assets/KoboldKare/Scenes/MainMap.unity", "Assets/KoboldKare/Scenes/ErrorScene.unity", "Assets/ThirdParty/Reiikz/Scenes/ReiikzMainMapAditions.unity"};
    private static string outputDirectory {
        get {
            string dir = Environment.GetEnvironmentVariable("BUILD_DIR");
            // string[] dirs = Directory.GetDirectories(dir);
            // int biggest=0, tmp=-1;
            // if(dirs.Length > 0){
            //     foreach(string d in dirs){
            //         try {
            //             tmp = Convert.ToInt32(Path.GetFileName(d));
            //         }catch(Exception e){}
            //         if(tmp > biggest){
            //             biggest = tmp;
            //         }
            //         tmp=-1;
            //     }
            // }
            // biggest++;
            // dir = dir + biggest;
            return dir.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        }
    }
    [MenuItem("KoboldKare/BuildLinux")]
    static void BuildLinux() {
        string output = outputDirectory+"Linux.x64"+Path.DirectorySeparatorChar+"KoboldKare";
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        Debug.Log("#### BUILDING TO " + output + "####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneLinux64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
    }

    [MenuItem("KoboldKare/BuildMac")]
    static void BuildMac() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = outputDirectory+"Mac"+Path.DirectorySeparatorChar+"KoboldKare.app";
        Debug.Log("#### BUILDING TO " + output + "####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneOSX, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
    }

    [MenuItem("KoboldKare/BuildWindows")]
    static void BuildWindows() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = outputDirectory+"Windows.x64"+Path.DirectorySeparatorChar+"KoboldKare.exe";
        Debug.Log("#### BUILDING TO " + output + "####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
    }

    [MenuItem("KoboldKare/BuildWindows32")]
    static void BuildWindows32() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        GetBuildVersion();
        string output = outputDirectory+"Windows.x86"+Path.DirectorySeparatorChar+"KoboldKare.exe";
        Debug.Log("#### BUILDING TO " + output + "####");
        var report = BuildPipeline.BuildPlayer(scenes, output, BuildTarget.StandaloneWindows, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
    }
    private static void GetBuildVersion() {
        string version = Environment.GetEnvironmentVariable("BUILD_NUMBER"); 
        string gitcommit = Environment.GetEnvironmentVariable("GIT_COMMIT").Substring(0,7); 
        if (!String.IsNullOrEmpty(version) && !String.IsNullOrEmpty(gitcommit)) {
            PlayerSettings.bundleVersion = version + "_" + gitcommit;
        } else if (!String.IsNullOrEmpty(version)) {
            PlayerSettings.bundleVersion = version;
        }
    }
}

#endif
