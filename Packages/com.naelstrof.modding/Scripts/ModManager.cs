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
                LoadMetaData($"{this.modPath}info.json");
                LoadPreview($"{this.modPath}preview.png");
            } catch (SystemException e) {
                Debug.LogException(e);
                Debug.LogError($"Failed to load mod at path {this.modPath}, from source {source}.");
            }
        }

        public bool enabled;
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
            return folderTitle != "SurfMap" && publishedFileId != (PublishedFileId_t)2934088282 && !string.IsNullOrEmpty(modPath) && !string.IsNullOrEmpty(catalogPath) && Directory.Exists(modPath) && File.Exists(catalogPath);
        }
        public string catalogPath {
            get {
                string searchDir = $"{modPath}{runningPlatform}";
                try {
                    foreach (var file in Directory.EnumerateFiles(searchDir)) {
                        if (file.EndsWith(".json")) {
                            return file;
                        }
                    }
                } catch {
                    return null;
                }
                return null;
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
                LoadMetaData($"{modPath}info.json");
                LoadPreview($"{modPath}preview.png");
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
            LoadMetaData($"{modPath}info.json");
            LoadPreview($"{modPath}preview.png");
        }
    }
    private static ModManager instance;
    private bool ready;
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
                modStubs.Add(new ModStub(mod.title, mod.publishedFileId, mod.modSource, mod.folderTitle, mod.description, mod.enabled,
                    mod.preview));
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
        public Texture2D preview;
        public string title;
        public string folderTitle;
        public string description;
        public ModSource source;
        public PublishedFileId_t id;
        public ModStub(string title, PublishedFileId_t id, ModSource source, string folderTitle, string description = "",  bool enabled = true, Texture2D preview = null)  {
            this.description = description;
            this.source = source;
            this.enabled = enabled;
            this.title = title;
            this.folderTitle = folderTitle;
            this.id = id;
            this.preview = preview;
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
    
    [SerializeReference,SerializeReferenceButton]
    private List<ModPostProcessor> modPostProcessors;


    public static bool GetFinishedLoading() {
        return instance.ready;
    }

    public static ReadOnlyCollection<ModStub> GetFullModList() {
        return ConvertToStubs(instance.fullModList, (info) => info.IsValid()).AsReadOnly();
    }

    public static async void SetModActive(ModStub stub, bool active) {
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

    public static void AddMod(string modPath) {
        instance.AddMod(new ModInfo(modPath, ModSource.SteamWorkshop));
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
        }
    }
    private async void AddMod(ModInfo info) {
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

                if (search.catalogPath == info.catalogPath) {
                    modFound = true;
                    break;
                }
            }

            if (modFound) {
                Debug.Log($"Already loaded mod with catalog path {info.catalogPath}, skipping...");
                return;
            }

            fullModList.Add(info);
            fullModList.Sort((a, b) => a.loadPriority.CompareTo(b.loadPriority));
        } finally {
            Mutex.Release();
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
            var mod = new ModInfo(node);
            if (mod.IsValid()) {
                AddMod(mod);
            }
        }

        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
    }
    private void ScanForNewMods() {
        string modCatalogPath = $"{Application.persistentDataPath}/{modLocation}";
        if (!Directory.Exists(modCatalogPath)) {
            Directory.CreateDirectory(modCatalogPath);
        }

        foreach (string directory in Directory.EnumerateDirectories(modCatalogPath)) {
            try {
                AddMod(new ModInfo(directory, ModSource.LocalModFolder));
            } catch (FileNotFoundException e) {
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

    private async Task LoadMods() {
        try {
            foreach (var modPostProcessor in modPostProcessors) {
                List<object> keys = new List<object>();
                var assets = Addressables.LoadResourceLocationsAsync(modPostProcessor.GetSearchLabel().RuntimeKey);
                await assets.Task;
                await modPostProcessor.LoadAllAssets(assets.Result);
            }
        } catch (Exception e) {
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
    }

    public static bool GetReady() => instance.ready;

    public static bool TryGetLastException(out Exception e) {
        if (instance.lastException != null) {
            e = instance.lastException;
            return true;
        }
        e = null;
        return false;
    }

    private void HandleException(AsyncOperationHandle handle, Exception e) {
        lastException = e;
    }

    private async void Start() {
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

        LoadConfig();
        ScanForNewMods();
        await ReloadMods();
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

    private async Task ReloadMods() {
        lastException = null;
        if (!IsValid()) {
            throw new UnityException("32 bit Windows does NOT support mods! Please upgrade your operating system!");
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
                var loader = Addressables.LoadContentCatalogAsync(modInfo.catalogPath);
                await loader.Task;
                if (!loader.IsDone || !loader.IsValid()) {
                    modInfo.enabled = false;
                } else {
                    modInfo.locator = loader.Result;
                }
            }
        } catch (Exception e) {
            lastException = e;
            throw;
        } finally {
            try {
                await LoadMods();
            } finally {
                Mutex.Release();
            }
        }
    }

    public static List<ModStub> GetLoadedMods() {
        return ConvertToStubs(instance.fullModList, (info) => info.enabled);
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
        var reloadModTask = instance.ReloadMods();
        yield return new WaitUntil(() => reloadModTask.IsCompleted);
        Debug.Log("Done reloading!");
    }
}
