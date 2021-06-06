using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor;
//using UnityEngine.AddressableAssets.Initialization;
//using UnityEngine.Localization;

public class Build {
    [MenuItem("KoboldKare/BuildLinux")]
    static void BuildLinux() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        //AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        ToggleSubstanceFiles(true);
        GetBuildVersion();
        Debug.Log("#### BUILDING ####");
        string[] scenes = {"Assets/Scenes/MainMenu.unity", "Assets/Scenes/MainMap.unity", "Assets/Scenes/ErrorScene.unity" };
        var report = BuildPipeline.BuildPlayer(scenes, "/var/lib/jenkins/workspace/KoboldKareLinux/Builds/KoboldKare", BuildTarget.StandaloneLinux64, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        ToggleSubstanceFiles(false);
    }

    [MenuItem("KoboldKare/BuildMac")]
    static void BuildMac() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        //AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        ToggleSubstanceFiles(true);
        GetBuildVersion();
        Debug.Log("#### BUILDING ####");
        string[] scenes = {"Assets/Scenes/MainMenu.unity", "Assets/Scenes/MainMap.unity", "Assets/Scenes/ErrorScene.unity" };
        var report = BuildPipeline.BuildPlayer(scenes, "/var/lib/jenkins/workspace/KoboldKareMac/Builds/KoboldKare.app", BuildTarget.StandaloneOSX, BuildOptions.None);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        ToggleSubstanceFiles(false);
    }

    [MenuItem("KoboldKare/BuildWindows")]
    static void BuildWindows() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        //AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        ToggleSubstanceFiles(true);
        GetBuildVersion();
        Debug.Log("#### BUILDING ####");
        string[] scenes = {"Assets/Scenes/MainMenu.unity", "Assets/Scenes/MainMap.unity", "Assets/Scenes/ErrorScene.unity" };
        var report = BuildPipeline.BuildPlayer(scenes, "C:/Windows/System32/config/systemprofile/AppData/Local/Jenkins.jenkins/workspace/KoboldKareWindows/Builds/KoboldKare.exe", BuildTarget.StandaloneWindows64, BuildOptions.Development);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        ToggleSubstanceFiles(false);
    }

    [MenuItem("KoboldKare/BuildWindows32")]
    static void BuildWindows32() {
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        //AddressableAssetSettings.CleanPlayerContent(AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);
        AddressableAssetSettings.BuildPlayerContent();
        ToggleSubstanceFiles(true);
        GetBuildVersion();
        Debug.Log("#### BUILDING ####");
        string[] scenes = {"Assets/Scenes/MainMenu.unity", "Assets/Scenes/MainMap.unity", "Assets/Scenes/ErrorScene.unity" };
        var report = BuildPipeline.BuildPlayer(scenes, "C:/Windows/System32/config/systemprofile/AppData/Local/Jenkins.jenkins/workspace/KoboldKareWindows/Builds/KoboldKare.exe", BuildTarget.StandaloneWindows, BuildOptions.Development);
        Debug.Log("#### BUILD DONE ####");
        Debug.Log(report.summary);
        ToggleSubstanceFiles(false);
    }
    private static void GetBuildVersion() {
        string version = Environment.GetEnvironmentVariable("BUILD_NUMBER"); 
        string gitcommit = Environment.GetEnvironmentVariable("GIT_COMMIT"); 
        if (!String.IsNullOrEmpty(version) && !String.IsNullOrEmpty(gitcommit)) {
            PlayerSettings.bundleVersion = version + "_" + gitcommit;
        } else if (!String.IsNullOrEmpty(version)) {
            PlayerSettings.bundleVersion = version;
        }
    }
    private static void ToggleSubstanceFiles(bool smallify) {
        // Probably totally broken, can't rely on substance plugin for now.
        /*string path = "Assets/Materials/MapMaterials";
        foreach(string file in Directory.GetFiles(path)) {
            AssetImporter importer = AssetImporter.GetAtPath(file);
            if (importer != null && file.Contains("sbsar")) {
                string metaPath = AssetDatabase.GetTextMetaFilePathFromAssetPath(file);
                StreamReader reader = new StreamReader(metaPath); 
                string data = reader.ReadToEnd();
                reader.Close();
                if (smallify) {
                    data = data.Replace("textureWidth: 1024", "textureWidth: 32");
                    data = data.Replace("textureHeight: 1024", "textureHeight: 32");
                    data = data.Replace("value=\\\"10,10\\\"", "value=\\\"5,5\\\"");
                } else {
                    data = data.Replace("textureWidth: 32", "textureWidth: 1024");
                    data = data.Replace("textureHeight: 32", "textureHeight: 1024");
                    data = data.Replace("value=\\\"5,5\\\"", "value=\\\"10,10\\\"");
                }
                StreamWriter writer = new StreamWriter(metaPath, false); 
                writer.Write(data);
                writer.Close();
                //AssetDatabase.ImportAsset(file);
            }
        }
        AssetDatabase.Refresh();*/
    }
}

#endif
