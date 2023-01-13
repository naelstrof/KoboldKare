using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ModManager : MonoBehaviour {
    private static ModManager instance;
    private bool ready;
    private List<ModInfo> modList;
    private Dictionary<IResourceLocation, AsyncOperationHandle<object>> loadedAssetHandles;
    private const string modLocation = "mods/";
    private const string JSONLocation = "modList.json";

    public delegate void ModReadyAction();
    private event ModReadyAction finishedLoading;
    
    [SerializeReference,SerializeReferenceButton]
    private List<ModPostProcessor> modPostProcessors;


    public static bool GetFinishedLoading() {
        return instance.ready;
    }

    public static void AddFinishedLoadingListener(ModReadyAction action) {
        instance.finishedLoading += action;
    }

    public static void RemoveFinishedLoadingListener(ModReadyAction action) {
        instance.finishedLoading -= action;
    }

    private void AddMod(ModInfo info) {
        bool modFound = false;
        foreach (var search in modList) {
            if (search.modName == info.modName) {
                modFound = true;
                break;
            }
        }
        if (modFound) {
            throw new Exception($"Mod already existed with name {info.modName}, skipping...");
        }
        modList.Add(info);
    }

    private void LoadModListFromJson() {
        modList.Clear();
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
                AddMod(new ModInfo() {
                    enabled = false,
                    modName = info.Name,
                    cataloguePath = filePath
                });
            }
        }
    }


    private IEnumerator LoadMods() {
        foreach (var modPostProcessor in modPostProcessors) {
            var assets = Addressables.LoadResourceLocationsAsync(modPostProcessor.searchLabel.RuntimeKey);
            yield return assets;
            foreach (var location in assets.Result) {
                var opHandle = Addressables.LoadAssetAsync<object>(location.PrimaryKey);
                yield return opHandle;
                modPostProcessor.LoadAsset(location, opHandle.Result);
                loadedAssetHandles.Add(location, opHandle);
            }
        }

        ready = true;
        finishedLoading?.Invoke();
    }

    private IEnumerator UnloadMods() {
        foreach (var modPostProcessor in modPostProcessors) {
            var assets = Addressables.LoadResourceLocationsAsync(modPostProcessor.searchLabel.RuntimeKey);
            yield return assets;
            foreach (var location in assets.Result) {
                if (!loadedAssetHandles.ContainsKey(location)) {
                    continue;
                }
                var assetHandle = loadedAssetHandles[location];
                modPostProcessor.UnloadAsset(location, assetHandle.Result);
                Addressables.Release(assetHandle);
                loadedAssetHandles.Remove(location);
            }
        }
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        instance = this;
        modList = new List<ModInfo>();
        loadedAssetHandles = new Dictionary<IResourceLocation, AsyncOperationHandle<object>>();
    }

    private IEnumerator ReloadMods() {
        yield return UnloadMods();
        foreach (var modInfo in modList) {
            if (!modInfo.enabled) {
                continue;
            }
            var loader = Addressables.LoadContentCatalogAsync(modInfo.cataloguePath);
            yield return loader;
            modInfo.locator = loader.Result;
        }
        yield return LoadMods();
    }

    public IEnumerator Start() {
        LoadModListFromJson();
        ScanForNewMods();
        yield return ReloadMods();
    }
}
