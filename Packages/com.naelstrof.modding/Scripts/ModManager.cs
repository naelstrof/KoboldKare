using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJSON;
using Steamworks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.SceneManagement;

public class ModManager : MonoBehaviour {
    private static ModManager instance;
    private bool ready;
    private Mutex sharedResource;
    private List<ModInfo> fullModList;
    private const string modLocation = "mods/";
    private const string JSONLocation = "modList.json";
    public struct ModStub {
        public string title;
        public PublishedFileId_t id;
        public ModStub(string title, PublishedFileId_t id) {
            this.title = title;
            this.id = id;
        }
    }

    public static string currentLoadingMod = "<currentLoadingMod>";
    public static string runningPlatform {
        get {
            switch (Application.platform) {
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxServer:
                case RuntimePlatform.LinuxEditor:
                    return "StandaloneLinux64";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsServer:
                    return IntPtr.Size == 8 ? "StandaloneWindows64" : "StandaloneWindows";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXServer:
                    return "StandaloneOSX";
                default: return "Unknown";
            }
        }
    }

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

    public static void AddMod(string modPath) {
        instance.AddMod(new ModInfo(modPath, ModInfo.ModSource.SteamWorkshop));
    }

    public static void RemoveMod(string modPath) {
        for (int i = 0; i < instance.fullModList.Count; i++) {
            if (instance.fullModList[i].modPath == modPath) {
                instance.fullModList.RemoveAt(i);
            }
        }
    }

    private void AddMod(ModInfo info) {
        bool modFound = false;
        foreach (var search in fullModList) {
            if (search.cataloguePath == info.cataloguePath) {
                modFound = true;
                break;
            }
        }
        if (modFound) {
            throw new Exception($"Mod already existed with catalog path {info.cataloguePath}, skipping...");
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
            AddMod(new ModInfo(node));
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

        foreach (string directory in Directory.EnumerateDirectories(modCatalogPath)) {
            AddMod(new ModInfo(directory, ModInfo.ModSource.LocalModFolder));
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

    private void UnloadMods() {
        try {
            sharedResource.WaitOne();
            foreach (var modPostProcessor in modPostProcessors) {
                modPostProcessor.UnloadAllAssets();
            }

            foreach (var mod in fullModList) {
                if (mod.locator == null) {
                    continue;
                }
                Addressables.RemoveResourceLocator(mod.locator);
                mod.locator = null;
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
        
        Addressables.InternalIdTransformFunc += location => {
            if (location.InternalId.Contains("<currentLoadingMod>")) {
                return location.InternalId.Replace("<currentLoadingMod>", currentLoadingMod);
            }
            return location.InternalId;
        };

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
        UnloadMods();
        try {
            sharedResource.WaitOne();
            foreach (var modInfo in fullModList) {
                if (!modInfo.enabled) {
                    continue;
                }

                AddressablesRuntimeProperties.ClearCachedPropertyValues(); 
                currentLoadingMod = $"{modInfo.modPath}{Path.DirectorySeparatorChar}";
                var loader = Addressables.LoadContentCatalogAsync(modInfo.cataloguePath);
                await loader.Task;
                modInfo.locator = loader.Result;
            }
        } finally {
            sharedResource.ReleaseMutex();
        }
        await LoadMods();
    }

    public static List<ModStub> GetLoadedMods() {
        List<ModStub> loadedMods = new List<ModStub>();
        foreach (var mod in GetFullModList()) {
            if (mod.enabled) {
                loadedMods.Add(new ModStub(mod.title, mod.publishedFileId));
            }
        }
        return loadedMods;
    }

    public static bool HasModsLoaded(List<ModStub> stubs) {
        foreach (var stub in stubs) {
            bool found = false;
            foreach (var mod in instance.fullModList) {
                if (mod.title == stub.title && mod.publishedFileId == stub.id) {
                    found = true;
                    if (!mod.enabled) {
                        return false;
                    }

                    break;
                }
            }

            if (!found) {
                return false;
            }
        }

        return true;
    }

    public static IEnumerator SetLoadedMods(ICollection<ModStub> stubs) {
        Debug.Log("Loading mod stubs...");
        List<ModStub> neededMods = new List<ModStub>(stubs);
        for(int i=0;i<neededMods.Count;i++) {
            if (neededMods[i].id != PublishedFileId_t.Invalid) {
                continue;
            }

            foreach (var mod in instance.fullModList) {
                if (mod.title != neededMods[i].title) continue;
                neededMods.RemoveAt(i);
                break;
            }
        }
        // Now we have a list of needed mods
        foreach (var modStub in neededMods) {
            if (modStub.id == PublishedFileId_t.Invalid) {
                throw new UnityException($"Couldn't find mod with name and id {modStub.title}, {modStub.id}. Can't continue! Tell the mod creator to upload it to Steam, or you need to manually install it.");
            }
        }

        foreach (var mod in instance.fullModList) {
            mod.enabled = false;
        }

        PublishedFileId_t[] fileIds = new PublishedFileId_t[neededMods.Count];
        yield return SteamWorkshopModLoader.TryDownloadAllMods(fileIds);
        foreach (var modStub in stubs) {
            bool found = false;
            foreach(var mod in instance.fullModList) {
                if (mod.title == modStub.title && mod.publishedFileId == modStub.id) {
                    found = true;
                    mod.enabled = true;
                    break;
                }
            }

            if (!found) {
                throw new UnityException($"Couldn't find mod with name and id {modStub.title}, {modStub.id}. Can't continue! It must have failed to download from the steam workshop, try logging into Steam!");
            }
        }

        Debug.Log("Reloading mods after acquiring stubs...");
        var reloadModTask = Reload();
        yield return new WaitUntil(() => reloadModTask.IsCompleted);
        Debug.Log("Done reloading!");
    }

    public static async Task Reload() {
        await instance.ReloadMods();
    }
}
