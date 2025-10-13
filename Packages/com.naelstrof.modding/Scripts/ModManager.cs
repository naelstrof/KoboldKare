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
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

public class ModManager : MonoBehaviour {
    public enum ModSource {
        LocalModFolder,
        SteamWorkshop,
        Any,
    }
    private class ModInfo {

        public ModInfo(JSONNode node) {
            Load(node);
        }
        
        public ModInfo(string modPath, ModSource source) {
            enabled = false;
            SetModPath(modPath);
            modSource = source;
            folderTitle = Path.GetFileName(Path.GetDirectoryName(modPath));
            try {
                LoadMetaData(infoPath);
                LoadPreview(previewPath);
            } catch {
                Debug.LogError($"Failed to load mod at path {this.modPath}, from source {source}.");
                throw;
            }
        }

        public bool enabled;
        public bool causedException = false;
        public string title;
        public string folderTitle;
        public PublishedFileId_t publishedFileId;
        public string description;
        public string modPath { private set; get; }
        public void SetModPath(string newPath) {
            string fullPath = newPath;
            if (!fullPath.EndsWith(Path.DirectorySeparatorChar) && !fullPath.EndsWith(Path.AltDirectorySeparatorChar)) {
                if (fullPath.Contains(Path.AltDirectorySeparatorChar)) {
                    fullPath += Path.AltDirectorySeparatorChar;
                } else {
                    fullPath += Path.DirectorySeparatorChar;
                }
            }
            modPath = fullPath;
        }

        public float loadPriority;
        public Texture2D preview;

        public bool IsValid() {
            if (!TryGetCatalogPath(out var catalogPath)) {
                return false;
            }
            return folderTitle != "SurfMap" && publishedFileId != (PublishedFileId_t)2934088282 && !string.IsNullOrEmpty(modPath) && !string.IsNullOrEmpty(catalogPath) && Directory.Exists(modPath) && File.Exists(catalogPath);
        }

        public bool TryGetCatalogPath(out string path) {
            string searchDir = $"{modPath}{runningPlatform}";
            if (!Directory.Exists(searchDir)) {
                path = "";
                return false;
            }
            foreach (var file in Directory.EnumerateFiles(searchDir)) {
                if (file.EndsWith(".json")) {
                    path = file;
                    return true;
                }
            }
            path = "";
            return false;
        }
        
        public string previewPath {
            get {
                string searchDir = $"{modPath}";
                if (!searchDir.EndsWith(Path.DirectorySeparatorChar) && !searchDir.EndsWith(Path.AltDirectorySeparatorChar)) {
                    if (searchDir.Contains(Path.AltDirectorySeparatorChar)) {
                        searchDir += Path.AltDirectorySeparatorChar;
                    } else {
                        searchDir += Path.DirectorySeparatorChar;
                    }
                }
                return searchDir + "preview.png";
            }
        }
        
        public string infoPath {
            get {
                string searchDir = $"{modPath}";
                if (!searchDir.EndsWith(Path.DirectorySeparatorChar) && !searchDir.EndsWith(Path.AltDirectorySeparatorChar)) {
                    if (searchDir.Contains(Path.AltDirectorySeparatorChar)) {
                        searchDir += Path.AltDirectorySeparatorChar;
                    } else {
                        searchDir += Path.DirectorySeparatorChar;
                    }
                }
                return searchDir + "info.json";
            }
        }

        public IResourceLocator locator;
        public ModSource modSource;

        private void LoadMetaData(string jsonPath) {
            using FileStream file = new FileStream(jsonPath, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(file);
            var rootNode = JSONNode.Parse(reader.ReadToEnd());
            if (rootNode.HasKey("publishedFileId")) {
                if (ulong.TryParse(rootNode["publishedFileId"], out ulong output)) {
                    publishedFileId = (PublishedFileId_t)output;
                }
            }
            if (rootNode.HasKey("description")) {
                description =rootNode["description"];
            }
            
            if (rootNode.HasKey("title")) {
                title = rootNode["title"];
            }
            if (rootNode.HasKey("loadPriority")) {
                loadPriority = rootNode["loadPriority"];
            }
            folderTitle = new DirectoryInfo(modPath).Name;
        }
        private void LoadPreview(string previewPngPath) {
            preview = new Texture2D(16, 16);
            preview.LoadImage(File.ReadAllBytes(previewPngPath));
        }

        public void Save(JSONNode node) {
            node["enabled"] = enabled;
            node["folderTitle"] = folderTitle;
            node["publishedFileId"] = publishedFileId.ToString();
            node["loadedFromSteam"] = modSource == ModSource.SteamWorkshop;
        }

        public void Refresh() {
            if (!string.IsNullOrEmpty(modPath)) {
                LoadMetaData(infoPath);
                LoadPreview(previewPath);
            }
        }

        public void Load(JSONNode node) {
            enabled = node["enabled"];
            if (ulong.TryParse(node["publishedFileId"], out ulong output)) {
                publishedFileId = (PublishedFileId_t)output;
            }
            modSource = node["loadedFromSteam"].AsBool ? ModSource.SteamWorkshop : ModSource.LocalModFolder;
            folderTitle = node["folderTitle"];
            if (modSource == ModSource.SteamWorkshop && publishedFileId != PublishedFileId_t.Invalid) {
                bool hasData = SteamUGC.GetItemInstallInfo(publishedFileId, out ulong punSizeOnDisk, out string pchFolder, 1024, out uint punTimeStamp);
                if (!hasData) {
                    return;
                }
                SetModPath(pchFolder);
            } else {
                SetModPath(string.IsNullOrEmpty(folderTitle) ? "" : $"{Application.persistentDataPath}/mods/{folderTitle}/");
            }

            if (string.IsNullOrEmpty(modPath)) return;
            LoadMetaData(infoPath);
            LoadPreview(previewPath);
        }
    }
    private static ModManager instance;
    private bool ready;
    private bool failedToLoadMods = false;
    private Exception lastException;
    private List<ModInfo> fullModList;
    private List<ModStub> playerConfig;
    public static List<ModStub> GetPlayerConfig() => instance.playerConfig;
    private const string modLocation = "mods/";
    private const string JsonFilename = "modList.json";

    private delegate bool ModConditional(ModInfo info);
    private static List<ModStub> ConvertToStubs(ICollection<ModInfo> infos, ModConditional conditional = null) {
        List<ModStub> modStubs = new List<ModStub>();
        foreach (var mod in infos) {
            if (conditional == null || conditional.Invoke(mod)) {
                modStubs.Add(new ModStub(mod.title, mod.publishedFileId, mod.modSource, mod.folderTitle, mod.causedException, mod.description, mod.enabled, mod.preview));
            }
        }
        return modStubs;
    }

    private static string jsonFolder {
        get {
            var path = $"{Application.persistentDataPath}/defaultUser/";
            if (SteamManager.Initialized) {
                path = $"{Application.persistentDataPath}/{SteamUser.GetSteamID().ToString()}/";
            }
            return path;
        }
    }
    private static string jsonLocation {
        get {
            var path = $"{Application.persistentDataPath}/defaultUser/{JsonFilename}";
            if (SteamManager.Initialized) {
                path = $"{Application.persistentDataPath}/{SteamUser.GetSteamID().ToString()}/{JsonFilename}";
            }
            return path;
        }
    }

    private static readonly SemaphoreSlim Mutex = new(1);
    public struct ModStub {
        public bool enabled;
        public bool causedException;
        public Texture2D preview;
        public string title;
        public string folderTitle;
        public string description;
        public ModSource source;
        public PublishedFileId_t id;
        public ModStub(string title, PublishedFileId_t id, ModSource source, string folderTitle, bool causedException = false, string description = "",  bool enabled = true, Texture2D preview = null)  {
            this.description = description;
            this.source = source;
            this.enabled = enabled;
            this.title = title;
            this.folderTitle = folderTitle;
            this.id = id;
            this.preview = preview;
            this.causedException = causedException;
        }
        public void Save(JSONNode node) {
            node["enabled"] = enabled;
            node["folderTitle"] = folderTitle;
            node["title"] = title;
            node["publishedFileId"] = id.ToString();
            node["loadedFromSteam"] = source == ModSource.SteamWorkshop;
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
    private event ModReadyAction modListChanged;
    
    [SerializeReference,SerializeReferenceButton]
    private List<ModPostProcessor> modPostProcessors;


    public static bool GetFinishedLoading() {
        bool isLocked = Mutex.CurrentCount == 0;
        return instance.ready && !isLocked;
    }

    public static ReadOnlyCollection<ModStub> GetFullModList() {
        return ConvertToStubs(instance.fullModList, (info) => info.IsValid()).AsReadOnly();
    }

    public static async Task SetModActive(ModStub stub, bool active) {
        await Mutex.WaitAsync();
        try {
            foreach (var mod in instance.fullModList) {
                if (mod.title != stub.title || mod.publishedFileId != stub.id || mod.modSource != stub.source) continue;
                mod.enabled = active;
                break;
            }
        } finally{
            Mutex.Release();
        }
        await instance.ReloadMods();
        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
    }

    public static void AddFinishedLoadingListener(ModReadyAction action) {
        instance.finishedLoading += action;
    }

    public static void RemoveFinishedLoadingListener(ModReadyAction action) {
        instance.finishedLoading -= action;
    }
    
    public static void AddModListChangeListener(ModReadyAction action) {
        instance.modListChanged += action;
    }

    public static void RemoveModListChangeListener(ModReadyAction action) {
        instance.modListChanged -= action;
    }

    public static void AddMod(string modPath) {
        try {
            var mod = new ModInfo(modPath, ModSource.SteamWorkshop);
            instance.AddMod(mod);
        } catch (Exception e) {
            instance.lastException = e;
            Debug.LogException(e);
            Debug.LogError($"Failed to load mod {modPath}.");
        }
    }

    public static async void RemoveMod(string modPath) {
        await Mutex.WaitAsync();
        try {
            for (int i = 0; i < instance.fullModList.Count; i++) {
                if (instance.fullModList[i].modPath == modPath) {
                    instance.fullModList.RemoveAt(i);
                }
            }
        } finally {
            Mutex.Release();
            instance.modListChanged?.Invoke();
        }
    }
    private async Task AddMod(ModInfo info) {
        if (info.publishedFileId == (PublishedFileId_t)2934088282) {
            Debug.Log("Skipping surfmap, as its now included in the base game...");
            return;
        }
        await Mutex.WaitAsync();
        try {
            bool modFound = false;
            foreach (var search in fullModList) {
                if (search.modSource == ModSource.SteamWorkshop &&
                    info.modSource == ModSource.SteamWorkshop &&
                    info.publishedFileId != PublishedFileId_t.Invalid &&
                    search.publishedFileId == info.publishedFileId) {
                    modFound = true;
                    // Possible that we only had a mod stub, so we update the path just in case.
                    search.SetModPath(info.modPath);
                    search.Refresh();
                    break;
                }

                if (!search.TryGetCatalogPath(out var catalogPathSearch)) {
                    if (!search.causedException) {
                        Debug.LogError($"{search.title} [{search.publishedFileId}] is missing a build folder for {runningPlatform}, can't load!");
                    }
                    search.causedException = true;
                    search.enabled = false;
                    continue;
                }

                if (!info.TryGetCatalogPath(out var catalogPathInfo)) {
                    if (!info.causedException) {
                        Debug.LogError($"{info.title} [{info.publishedFileId}] is missing a build folder for {runningPlatform}, can't load!");
                    }
                    info.causedException = true;
                    info.enabled = false;
                    continue;
                }

                if (catalogPathSearch == catalogPathInfo) {
                    modFound = true;
                    break;
                }
            }

            if (modFound) {
                Debug.Log($"Already have mod {info.title} [{info.publishedFileId}], skipping...");
                return;
            }

            fullModList.Add(info);
            fullModList.Sort((a, b) => a.loadPriority.CompareTo(b.loadPriority));
        } catch (Exception e) {
            lastException = e;
            info.causedException = true;
            info.enabled = false;
        } finally {
            Mutex.Release();
            modListChanged?.Invoke();
        }
    }

    private void LoadConfig() {
        fullModList.Clear();
        if (!Directory.Exists(jsonFolder)) {
            Directory.CreateDirectory(jsonFolder);
        }

        if (!File.Exists(jsonLocation)) {
            using FileStream quickWrite = File.Create(jsonLocation);
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
        if (!n.HasKey("modList")) return;
        JSONArray array = n["modList"].AsArray;
        if (array.Count == 0) {
            return;
        }
        foreach (var node in array) {
            try {
                var mod = new ModInfo(node);
                if (mod.IsValid()) {
                    AddMod(mod);
                }
            } catch (Exception e) {
                Debug.Log(e);
                instance.lastException = e;
                Debug.LogException(e);
                Debug.LogError($"Failed to load mod {node}.");
            }
        }

        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
        modListChanged?.Invoke();
    }
    private void ScanForNewMods() {
        string modCatalogPath = $"{Application.persistentDataPath}/{modLocation}";
        if (!Directory.Exists(modCatalogPath)) {
            Directory.CreateDirectory(modCatalogPath);
        }

        foreach (string directory in Directory.EnumerateDirectories(modCatalogPath)) {
            try {
                var mod = new ModInfo(directory, ModSource.LocalModFolder);
                AddMod(mod);
            } catch (Exception e) {
                lastException = e;
                Debug.LogException(e);
                Debug.LogError($"Failed to load mod {directory}.");
            }
        }
    }

    public static void SaveConfig() {
        if (instance.playerConfig == null) {
            return;
        }

        JSONNode rootNode = JSONNode.Parse("{}");
        JSONNode nodeArray = new JSONArray();
        foreach(var mod in instance.playerConfig) {
            if (!mod.enabled) continue;
            JSONNode node = JSONNode.Parse("{}");
            mod.Save(node);
            nodeArray.Add(node);
        }
        rootNode["modList"] = nodeArray;
        using FileStream quickWrite = File.Create(jsonLocation);
        var chars = rootNode.ToString(2);
        quickWrite.Write(Encoding.UTF8.GetBytes(chars),0,chars.Length);
        quickWrite.Close();
    }

    private ModInfo currentInspectedMod = null;
    private async Task InspectMods() {
        foreach (var modInfo in instance.fullModList) {
            if (!modInfo.enabled) {
                if (modInfo.locator != null) {
                    Addressables.RemoveResourceLocator(modInfo.locator);
                    modInfo.locator = null;
                }
                continue;
            }
            currentInspectedMod = modInfo;
            var locator = modInfo.locator;
            var keys = locator.Keys;
            List<object> filteredKeys = new List<object>();
            foreach (var key in keys) {
                if (key is String keyString && !keyString.EndsWith("bundle")) {
                    filteredKeys.Add(key);
                }
            }
            var handle = Addressables.LoadAssetsAsync<Object>(filteredKeys, OnInspect, Addressables.MergeMode.None);
            await handle.Task;
            if (modInfo.causedException) {
                failedToLoadMods = true;
                modInfo.enabled = false;
                Addressables.RemoveResourceLocator(modInfo.locator);
                modInfo.locator = null;
            }
            Addressables.Release(handle);
        }
        currentInspectedMod = null;
    }

    private void OnInspect(Object obj) {
        
    }

    private async Task LoadMods(bool shouldInspect) {
        if (shouldInspect) {
            await InspectMods();
            Resources.UnloadUnusedAssets();
        }

        try {
            foreach (var modPostProcessor in modPostProcessors) {
                var assets = Addressables.LoadResourceLocationsAsync(modPostProcessor.GetSearchLabel().RuntimeKey);
                await assets.Task;
                await modPostProcessor.LoadAllAssets(assets.Result);
                Addressables.Release(assets);
            }
        } catch (Exception e) {
            failedToLoadMods = true;
            lastException = e;
            throw;
        } finally {
            ready = true;
            finishedLoading?.Invoke();
        }
    }

    private void UnloadMods() {
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
        
        Resources.UnloadUnusedAssets();
    }

    public static bool GetReady() => GetFinishedLoading();

    public static bool GetFailedToLoadMods() {
        return instance.failedToLoadMods;
    }
    public static bool TryGetLastException(out Exception e) {
        if (instance.lastException != null) {
            e = instance.lastException;
            instance.lastException = null;
            return true;
        }
        e = null;
        return false;
    }

    private void HandleException(AsyncOperationHandle handle, Exception e) {
        if (currentInspectedMod != null) {
            currentInspectedMod.causedException = true;
        }
        lastException = e;
    }

    private void Start() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        ResourceManager.ExceptionHandler += HandleException;
        Addressables.InternalIdTransformFunc += location => {
            if (location.InternalId.Contains("<currentLoadingMod>")) {
                return location.InternalId.Replace("<currentLoadingMod>", currentLoadingMod);
            }
            return location.InternalId;
        };

        ready = false;
        instance = this;
        fullModList = new List<ModInfo>();
        foreach(var modPostProcessor in modPostProcessors) {
            modPostProcessor.Awake();
        }

        OnStart();
    }

    private async Task OnStart() {
        LoadConfig();
        ScanForNewMods();
        await ReloadMods(true);
        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
    }

    public static bool IsValid() {
        switch (Application.platform) {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsServer:
                if (IntPtr.Size != 8) {
                    return false;
                }
                break;
        }
        return true;
    }

    private struct ModLoaderPairs {
        public AsyncOperationHandle handle;
        public ModInfo modInfo;
    }
    
    
    private async Task ReloadMods(bool shouldInspect) {
        failedToLoadMods = false;
        if (!IsValid()) {
            failedToLoadMods = true;
            throw new UnityException("32 bit Windows does NOT support mods! Please upgrade your operating system!");
        }

        while (SteamWorkshopModLoader.IsBusy) {
            await Task.Delay(1000);
        }
        
        await Mutex.WaitAsync();
        ready = false;
        try {
            UnloadMods();
            foreach (var modInfo in fullModList) {
                if (!modInfo.enabled) {
                    continue;
                }

                AddressablesRuntimeProperties.ClearCachedPropertyValues();
                currentLoadingMod = $"{modInfo.modPath}{Path.DirectorySeparatorChar}";
                if (!modInfo.TryGetCatalogPath(out var catalogPath)) {
                    modInfo.enabled = false;
                    modInfo.causedException = true;
                    continue;
                }
                var loader = Addressables.LoadContentCatalogAsync(catalogPath);
                await loader.Task;
                if (!loader.IsDone || !loader.IsValid() || loader.Status == AsyncOperationStatus.Failed || loader.OperationException != null) {
                    modInfo.causedException = true;
                    modInfo.enabled = false;
                } else {
                    modInfo.locator = (IResourceLocator)loader.Result;
                }
            }
        } catch (Exception e) {
            failedToLoadMods = true;
            lastException = e;
            throw;
        } finally {
            try {
                await LoadMods(shouldInspect);
            } finally {
                Mutex.Release();
            }
        }
    }

    public static List<ModStub> GetLoadedMods() {
        return ConvertToStubs(instance.fullModList, (info) => info.enabled);
    }

    public static async Task AllModsSetActive(bool active) {
        await Mutex.WaitAsync();
        try {
            foreach (var mod in instance.fullModList) {
                mod.enabled = active;
            }
        } finally {
            Mutex.Release();
        }
        await instance.ReloadMods(true);
        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
    }

    public static bool HasModsLoaded(IList<ModStub> stubs) {
        int count = 0;
        foreach (var mod in instance.fullModList) {
            if (mod.enabled) {
                count++;
            }
        }

        if (stubs.Count != count) {
            return false;
        }

        foreach (var stub in stubs) {
            bool found = false;
            foreach (var mod in instance.fullModList) {
                if (mod.title != stub.title || mod.publishedFileId != stub.id) continue;
                found = true;
                if (!mod.enabled) {
                    return false;
                }
                break;
            }
            if (!found) {
                return false;
            }
        }

        return true;
    }

    public enum LoadModType {
        PlayerConfig,
        ServerConfig,
    }

    public static IEnumerator SetLoadedMods(IList<ModStub> stubs) {
        if (HasModsLoaded(stubs)) {
            yield break;
        }

        Debug.Log("Loading mod stubs...");
        List<ModStub> neededMods = new List<ModStub>(stubs);
        for(int i=0;i<neededMods.Count;i++) {
            if (neededMods[i].id != PublishedFileId_t.Invalid) {
                continue;
            }

            foreach (var mod in instance.fullModList) {
                if (mod.title != neededMods[i].title) continue;
                neededMods.RemoveAt(i--);
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
        for (int i = 0; i < neededMods.Count; i++) {
            fileIds[i] = neededMods[i].id;
        }
        yield return SteamWorkshopModLoader.TryDownloadAllMods(fileIds);
        foreach (var modStub in stubs) {
            bool found = false;
            foreach(var mod in instance.fullModList) {
                if (mod.title != modStub.title || mod.publishedFileId != modStub.id) continue;
                found = true;
                mod.enabled = true;
                break;
            }

            if (!found) {
                throw new UnityException($"Couldn't find mod with name and id {modStub.title}, {modStub.id}. Can't continue! It must have failed to download from the steam workshop, try logging into Steam!");
            }
        }

        Debug.Log("Reloading mods after acquiring stubs...");
        var reloadModTask = instance.ReloadMods(false);
        yield return new WaitUntil(() => reloadModTask.IsCompleted);
        Debug.Log("Done reloading!");
    }
}
