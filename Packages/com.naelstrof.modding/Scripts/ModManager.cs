using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public class ModManager : MonoBehaviour {
    private static ModManager instance;
    private bool ready;
    private Mutex sharedResource;
    private List<ModInfo> fullModList;
    private const string modLocation = "mods/";
    private const string JSONLocation = "modList.json";

    public delegate void ModReadyAction();
    private event ModReadyAction finishedLoading;
    
    [SerializeReference,SerializeReferenceButton]
    private List<ModPostProcessor> modPostProcessors;


    public static bool GetFinishedLoading() {
        return instance.ready;
    }

    public static ReadOnlyCollection<ModInfo> GetFullModList() {
        return instance.fullModList.AsReadOnly();
    }

    public static void AddFinishedLoadingListener(ModReadyAction action) {
        instance.finishedLoading += action;
    }

    public static void RemoveFinishedLoadingListener(ModReadyAction action) {
        instance.finishedLoading -= action;
    }

    private void AddMod(ModInfo info) {
        bool modFound = false;
        foreach (var search in fullModList) {
            if (search.modName == info.modName) {
                modFound = true;
                break;
            }
        }
        if (modFound) {
            throw new Exception($"Mod already existed with name {info.modName}, skipping...");
        }
        fullModList.Add(info);
    }

    private void LoadModListFromJson() {
        fullModList.Clear();
        string jsonLocation = $"{Application.persistentDataPath}/{JSONLocation}";
        if (!File.Exists(jsonLocation)) {
            using FileStream quickWrite = File.Open(jsonLocation, FileMode.CreateNew);
            byte[] write = { (byte)'{', (byte)'}', (byte)'\n' };
            quickWrite.Write(write, 0, write.Length);
            quickWrite.Close();
        }

        using FileStream file = File.Open(jsonLocation, FileMode.Open);
        byte[] b = new byte[file.Length];
        file.Read(b,0,(int)file.Length);
        file.Close();
        string data = Encoding.UTF8.GetString(b);
        JSONNode n = JSON.Parse(data);
        foreach (var node in n) {
            ModInfo info = new ModInfo();
            info.Load(n);
            AddMod(info);
        }
    }

    public static void IncrementPriority(ModInfo info) {
        int index = instance.fullModList.IndexOf(info);
        int desiredIndex = Mathf.Max(index - 1,0);
        (instance.fullModList[index], instance.fullModList[desiredIndex]) =
            (instance.fullModList[desiredIndex], instance.fullModList[index]);
    }
    public static void DecrementPriority(ModInfo info) {
        int index = instance.fullModList.IndexOf(info);
        int desiredIndex = Mathf.Min(index + 1,instance.fullModList.Count-1);
        (instance.fullModList[index], instance.fullModList[desiredIndex]) =
            (instance.fullModList[desiredIndex], instance.fullModList[index]);
    }

    private void ScanForNewMods() {
        string modCatalogPath = $"{Application.persistentDataPath}/{modLocation}";
        if (!Directory.Exists(modCatalogPath)) {
            Directory.CreateDirectory(modCatalogPath);
        }

        foreach (string modPath in Directory.EnumerateDirectories(modCatalogPath)) {
            foreach (string filePath in Directory.EnumerateFiles(modPath)) {
                if (!filePath.EndsWith(".json")) {
                    continue;
                }
                DirectoryInfo info = new DirectoryInfo(modPath);
                AddMod(new ModInfo {
                    enabled = false,
                    modName = info.Name,
                    cataloguePath = filePath
                });
            }
        }
    }

    private async Task LoadMods() {
        try {
            sharedResource.WaitOne();
            foreach (var modPostProcessor in modPostProcessors) {
                var assets = Addressables.LoadResourceLocationsAsync(modPostProcessor.GetSearchLabel().RuntimeKey);
                await assets.Task;
                await modPostProcessor.LoadAllAssets(assets.Result);
            }
        } finally {
            sharedResource.ReleaseMutex();
        }
        ready = true;
        finishedLoading?.Invoke();
    }

    private async Task UnloadMods() {
        try {
            sharedResource.WaitOne();
            foreach (var modPostProcessor in modPostProcessors) {
                var assets = Addressables.LoadResourceLocationsAsync(modPostProcessor.GetSearchLabel().RuntimeKey);
                await assets.Task;
                modPostProcessor.UnloadAllAssets(assets.Result);
            }
        } finally {
            sharedResource.ReleaseMutex();
        }
    }

    public static bool GetReady() => instance.ready;

    private async void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        ready = false;
        sharedResource = new Mutex();
        instance = this;
        fullModList = new List<ModInfo>();
        foreach(var modPostProcessor in modPostProcessors) {
            modPostProcessor.Awake();
        }

        LoadModListFromJson();
        ScanForNewMods();
        await ReloadMods();
    }

    private async Task ReloadMods() {
        ready = false;
        await UnloadMods();
        try {
            sharedResource.WaitOne();
            foreach (var modInfo in fullModList) {
                if (!modInfo.enabled) {
                    continue;
                }

                var loader = Addressables.LoadContentCatalogAsync(modInfo.cataloguePath);
                await loader.Task;
                modInfo.locator = loader.Result;
            }
        } finally {
            sharedResource.ReleaseMutex();
        }
        await LoadMods();
    }

    public static async void Reload() {
        await instance.ReloadMods();
    }
}
