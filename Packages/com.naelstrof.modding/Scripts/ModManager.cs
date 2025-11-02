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

    private ModStatus status = ModStatus.Initializing;

    public enum ModStatus {
        Initializing,
        WaitingForDownloads,
        ScanningForMods,
        UnloadingMods,
        LoadingMods,
        LoadingAssets,
        InspectingForErrors,
        Ready,
    }

    public struct ModInfoData {
        public string GetSceneBundleLocation() {
            return $"{directoryInfo.FullName}/{runningPlatform}/{assets["SceneBundleName"].ToString().Trim('"')}";
        }
        public static bool TryGetModInfoData(string jsonPath, ModSource source, out ModInfoData data) {
            FileInfo fileInfo = new FileInfo(jsonPath);
            if (!fileInfo.Exists) {
                Debug.LogError($"Failed to load mod {jsonPath}, file does not exist.");
                data = default;
                return false;
            }

            DirectoryInfo directoryInfo = fileInfo.Directory;
            if (directoryInfo == null) {
                Debug.LogError($"Failed to load mod {jsonPath}, file does not exist.");
                data = default;
                return false;
            }

            ModInfoData modInfoData = new ModInfoData {
                directoryInfo = directoryInfo,
                source = source,
                assets = JSONNode.Parse("{}")
            };

            using FileStream file = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
            using StreamReader reader = new StreamReader(file);
            JSONNode rootNode;
            try {
                rootNode = JSONNode.Parse(reader.ReadToEnd());
            } catch (Exception e) {
                Debug.LogError($"Failed to parse mod info json {jsonPath}.");
                Debug.LogException(e);
                data = default;
                return false;
            }

            if (rootNode.HasKey("publishedFileId")) {
                if (ulong.TryParse(rootNode["publishedFileId"], out ulong output)) {
                    modInfoData.publishedFileId = (PublishedFileId_t)output;
                }
            }

            if (rootNode.HasKey("description")) {
                modInfoData.description = rootNode["description"];
            }

            if (rootNode.HasKey("title")) {
                modInfoData.title = rootNode["title"];
            } else {
                modInfoData.title = directoryInfo.Name;
            }

            if (rootNode.HasKey("loadPriority")) {
                modInfoData.loadPriority = rootNode["loadPriority"];
            }

            if (rootNode.HasKey("bundle")) {
                modInfoData.assets = rootNode["bundle"];
            }

            if (rootNode.HasKey("version")) {
                modInfoData.version = rootNode["version"];
                if (modInfoData.version == "v0.0.1") {
                    
                } else {
                    Debug.LogError($"Failed to load mod {jsonPath}, unknown version {modInfoData.version}.");
                    data = default;
                    return false;
                }
            } else {
                modInfoData.version = "v0.0.0";
            }
            
            FileInfo previewPath = new FileInfo($"{directoryInfo.FullName}/preview.png");
            if (previewPath.Exists) {
                modInfoData.preview = new Texture2D(16, 16);
                modInfoData.preview.LoadImage(File.ReadAllBytes(previewPath.FullName));
            } else {
                modInfoData.preview = Texture2D.grayTexture;
            }

            data = modInfoData;
            return true;
        }

        public string title;
        public DirectoryInfo directoryInfo;
        public string version;
        public JSONNode assets;
        public PublishedFileId_t publishedFileId;
        public string description;
        public float loadPriority;
        public Texture2D preview;
        public ModSource source;
    }

    public abstract class Mod {
        public ModInfoData info;
        public bool enabled;
        public bool causedException = false;
        protected readonly SemaphoreSlim modMutex = new(1);

        protected Mod(ModInfoData info) {
            this.info = info;
        }

        public bool GetRepresentedByStub(ModStub stub) {
            return info.publishedFileId == stub.id && info.title == stub.title;
        }

        public virtual bool IsValid() {
            return true;
            //return info.publishedFileId != (PublishedFileId_t)2934088282;
        }

        public abstract bool GetAssetsLoaded();
        protected abstract Task TryLoad();
        protected abstract Task TryUnload();

        public abstract Task SetLoaded(bool active);
        public abstract Task SetAssetsAvailable(bool active);
        public abstract bool Provides(IResourceLocation location);
    }

    public class ModAssetBundle : Mod {
        public AssetBundle bundle;
        private bool loaded = false;
        private bool assetsLoaded = false;
        public ModAssetBundle(ModInfoData info) : base(info) {
        }
        private string bundleLocation => $"{info.directoryInfo.FullName}/{runningPlatform}/{info.assets["BundleName"].ToString().Trim('"')}";

        public override bool IsValid() {
            FileInfo bundleFileInfo = new FileInfo(bundleLocation);
            if (!bundleFileInfo.Exists) {
                Debug.LogError($"AssetBundle {bundleFileInfo.FullName} does not exist.");
                return false;
            }
            return base.IsValid();
        }

        public override bool GetAssetsLoaded() {
            return assetsLoaded;
        }

        protected override async Task TryLoad() {
            try {
                bundle = await AssetBundle.LoadFromFileAsync(bundleLocation).AsTask();
                if (!bundle) {
                    throw new Exception($"Failed to load bundle. {info.title} [{info.publishedFileId}]");
                } else {
                    loaded = true;
                }
            } catch (Exception e) {
                Debug.LogError($"Failed to load bundle for {info.title} [{info.publishedFileId}].");
                Debug.LogException(e);
                causedException = true;
                instance.changed = true;
                instance.lastException = e;
                throw;
            }
        }
        protected override async Task TryUnload() {
            if (!bundle) {
                return;
            }
            await bundle.UnloadAsync(true).AsTask();
            loaded = false;
            bundle = null;
        }

        public override async Task SetLoaded(bool active) {
            try {
                if (!loaded && active) {
                    instance.status = ModStatus.LoadingMods;
                    await TryLoad();
                } else if (loaded && !active) {
                    await TryUnload();
                }
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"Failed to set loaded state for mod {info.title} [{info.publishedFileId}].");
                instance.lastException = e;
                causedException = true;
                instance.changed = true;
                throw;
            } finally {
                instance.status = ModStatus.Ready;
                instance.ready = true;
            }
        }

        public override async Task SetAssetsAvailable(bool active) {
            await modMutex.WaitAsync();
            try {
                if (!assetsLoaded && active) {
                    await SetLoaded(true);
                    instance.status = ModStatus.LoadingAssets;
                    foreach (var modPostProcessor in instance.earlyModPostProcessors) {
                        await modPostProcessor.HandleAssetBundleMod(info, bundle);
                    }

                    foreach (var modPostProcessor in instance.modPostProcessors) {
                        await modPostProcessor.HandleAssetBundleMod(info, bundle);
                    }
                } else if (assetsLoaded && !active) {
                    foreach (var modPostProcessor in instance.earlyModPostProcessors) {
                        await modPostProcessor.UnloadAssets(info);
                    }

                    foreach (var modPostProcessor in instance.modPostProcessors) {
                        await modPostProcessor.UnloadAssets(info);
                    }
                    await SetLoaded(false);
                }
                assetsLoaded = active;
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"Failed to make assets available for mod {info.title} [{info.publishedFileId}].");
                instance.lastException = e;
                causedException = true;
                instance.changed = true;
                throw;
            } finally {
                instance.status = ModStatus.Ready;
                instance.ready = true;
                modMutex.Release();
            }
        }

        public override bool Provides(IResourceLocation location) {
            return false;
        }

        public AssetBundle GetBundle() {
            return bundle;
        }
    }

    public class ModAddressable : Mod {
        private bool loaded = false;
        private bool loadedAssets = false;
        public ModAddressable(ModInfoData info) : base(info) {
        }
        public override bool IsValid() {
            if (!TryGetCatalogPath(out var catalogPath)) {
                return false;
            }
            return base.IsValid();
        }

        public override bool GetAssetsLoaded() {
            return loadedAssets;
        }

        protected override async Task TryLoad() {
            if (!IsValid()) {
                return;
            }

            if (locator != null) {
                return;
            }

            AddressablesRuntimeProperties.ClearCachedPropertyValues();
            currentLoadingMod = $"{info.directoryInfo.FullName}{Path.DirectorySeparatorChar}";
            if (!TryGetCatalogPath(out var catalogPath)) {
                enabled = false;
                causedException = true;
                instance.changed = true;
            }

            var loader = Addressables.LoadContentCatalogAsync(catalogPath);
            await loader.Task;
            if (!loader.IsDone || !loader.IsValid() || loader.Status == AsyncOperationStatus.Failed ||
                loader.OperationException != null) {
                enabled = false;
                causedException = true;
                instance.changed = true;
            } else {
                locator = loader.Result;
            }

            Addressables.Release(loader);
            loaded = true;
        }

        public override async Task SetLoaded(bool active) {
            try {
                if (!loaded && active) {
                    await TryLoad();
                } else if (loaded && !active) {
                    await TryUnload();
                }
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"Failed to set active state for mod {info.title} [{info.publishedFileId}].");
                instance.lastException = e;
                causedException = true;
                instance.changed = true;
                throw;
            } finally {
                instance.status = ModStatus.Ready;
                instance.ready = true;
            }
        }

        public override async Task SetAssetsAvailable(bool active) {
            await modMutex.WaitAsync();
            try {
                if (!loadedAssets && active) {
                    var cancelTokenSource = new CancellationTokenSource();
                    cancelTokenSources.Add(cancelTokenSource);
                    try {
                        await SetLoaded(true);
                        instance.status = ModStatus.LoadingAssets;
                        foreach (var modPostProcessor in instance.earlyModPostProcessors) {
                            cancelTokenSource.Token.ThrowIfCancellationRequested();
                            await modPostProcessor.HandleAddressableMod(info, locator);
                        }

                        foreach (var modPostProcessor in instance.modPostProcessors) {
                            cancelTokenSource.Token.ThrowIfCancellationRequested();
                            await modPostProcessor.HandleAddressableMod(info, locator);
                        }
                    } finally {
                        cancelTokenSources.Remove(cancelTokenSource);
                    }
                } else if (loadedAssets && !active) {
                    instance.status = ModStatus.LoadingAssets;
                    foreach (var modPostProcessor in instance.earlyModPostProcessors) {
                        await modPostProcessor.UnloadAssets(info);
                    }

                    foreach (var modPostProcessor in instance.modPostProcessors) {
                        await modPostProcessor.UnloadAssets(info);
                    }
                    await SetLoaded(false);
                }
                loadedAssets = active;
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError($"Failed to set active state for mod {info.title} [{info.publishedFileId}].");
                instance.lastException = e;
                causedException = true;
                instance.changed = true;
                _ = TryUnload();
                throw;
            } finally {
                modMutex.Release();
                instance.status = ModStatus.Ready;
                instance.ready = true;
            }
        }

        protected override Task TryUnload() {
            if (locator == null) {
                return Task.CompletedTask;
            }

            Addressables.RemoveResourceLocator(locator);
            locator = null;
            loaded = false;
            return Task.CompletedTask;
        }

        public override bool Provides(IResourceLocation location) {
            if (locator == null) {
                return false;
            }
            return locator.Locate(location.PrimaryKey, typeof(Object), out var locations) && locations.Contains(location);
        }

        private bool TryGetCatalogPath(out string path) {
            string searchDir = $"{info.directoryInfo.FullName}/{runningPlatform}";
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

        public IResourceLocator GetLocator() => locator;
        private IResourceLocator locator;
    }
    
    private static ModManager instance;
    private bool ready;
    private bool failedToLoadMods = false;
    private Exception lastException;
    private List<Mod> fullModList;
    private List<ModStub> playerConfig;
    public static List<ModStub> GetPlayerConfig() => instance.playerConfig;
    private const string modLocation = "mods/";
    private const string JsonFilename = "modList.json";
    private bool changed = false;
    
    [SerializeReference,SerializeReferenceButton]
    private List<ModPostProcessor> earlyModPostProcessors;
    
    [SerializeReference,SerializeReferenceButton]
    private List<ModPostProcessor> modPostProcessors;
    
    public static bool GetChanged() => instance.changed;

    private delegate bool ModConditional(Mod info);
    private static List<ModStub> ConvertToStubs(ICollection<Mod> infos, ModConditional conditional = null) {
        List<ModStub> modStubs = new List<ModStub>();
        foreach (var mod in infos) {
            if (conditional == null || conditional.Invoke(mod)) {
                modStubs.Add(new ModStub(mod.info.title, mod.info.publishedFileId, mod.info.source, mod.info.directoryInfo.Name, mod.causedException, mod.info.description, mod.enabled, mod.info.preview));
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
        public float loadPriority;

        public ModStub(JSONNode node) {
            if (node.HasKey("enabled")) {
                enabled = node["enabled"];
            } else {
                enabled = false;
            }
            if (node.HasKey("folderTitle")) {
                folderTitle = node["folderTitle"];
            } else {
                folderTitle = "UnknownMod";
            }
            if (node.HasKey("title")) {
                title = node["title"];
            } else {
                title = folderTitle;
            }
            description = node.HasKey("description") ? node["description"] : "";
            causedException = false;
            preview = null;
            if (node.HasKey("publishedFileId")) {
                if (ulong.TryParse(node["publishedFileId"], out ulong output)) {
                    id = (PublishedFileId_t)output;
                } else {
                    id = PublishedFileId_t.Invalid;
                }
            } else {
                id = PublishedFileId_t.Invalid;
            }

            if (node.HasKey("loadedFromSteam")) {
                source = node["loadedFromSteam"] == true ? ModSource.SteamWorkshop : ModSource.LocalModFolder;
            } else {
                source = ModSource.Any;
            }
            loadPriority = 0f;
        }

        public ModStub(ModInfoData data) {
            this.description = data.description;
            this.source = data.source;
            this.enabled = true;
            this.title = data.title;
            this.folderTitle = data.directoryInfo.Name;
            this.id = data.publishedFileId;
            this.preview = data.preview;
            this.causedException = false;
            this.loadPriority = data.loadPriority;
        }
        public readonly bool GetRepresentedBy(ModInfoData data) {
            return data.publishedFileId == id && data.title == title;
        }
        public readonly bool GetRepresentedBy(ModStub stub) {
            return stub.id == id && stub.title == title;
        }
        public ModStub(string title, PublishedFileId_t id, ModSource source, string folderTitle, bool causedException = false, string description = "",  bool enabled = true, Texture2D preview = null, float loadPriority = 0f)  {
            this.description = description;
            this.source = source;
            this.enabled = enabled;
            this.title = title;
            this.folderTitle = folderTitle;
            this.id = id;
            this.preview = preview;
            this.causedException = causedException;
            this.loadPriority = loadPriority;
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
    private static string runningPlatform {
        get {
            switch (Application.platform) {
                case RuntimePlatform.LinuxPlayer:
                case RuntimePlatform.LinuxServer:
                case RuntimePlatform.LinuxEditor:
                    return "StandaloneLinux64";
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsServer:
                    return "StandaloneWindows64";
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

    public static bool GetFinishedLoading() {
        bool isLocked = Mutex.CurrentCount == 0;
        return instance.ready && !isLocked;
    }

    public static ReadOnlyCollection<ModStub> GetFullModList() {
        return ConvertToStubs(instance.fullModList, (info) => info.IsValid()).AsReadOnly();
    }

    public static async Task SetModEnabled(ModStub stub, bool active) {
        await Mutex.WaitAsync();
        try {
            bool found = false;
            foreach (var mod in instance.fullModList) {
                if (!mod.GetRepresentedByStub(stub)) continue;
                instance.changed = true;
                instance.ready = false;
                instance.status = ModStatus.LoadingAssets;
                found = true;
                await mod.SetAssetsAvailable(active);
                mod.enabled = active;
                break;
            }

            if (!found) {
                Debug.LogError($"Failed to find mod {stub.title} [{stub.id}], this is really weird.");
            }
        } finally{
            Mutex.Release();
            instance.ready = true;
            instance.status = ModStatus.Ready;
            instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
            instance.modListChanged?.Invoke();
            instance.finishedLoading?.Invoke();
        }
    }
    
    public static async Task SetModAssetsAvailable(ModStub stub, bool loaded) {
        await Mutex.WaitAsync();
        try {
            bool found = false;
            foreach (var mod in instance.fullModList) {
                if (!mod.GetRepresentedByStub(stub)) continue;
                found = true;
                await mod.SetAssetsAvailable(loaded);
                break;
            }

            if (!found) {
                Debug.LogError($"Failed to set assets for mod {stub.title} [{stub.id}], this is really weird.");
            }
        } finally{
            Mutex.Release();
        }
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
            if (!ModInfoData.TryGetModInfoData(modPath + "/info.json", ModSource.SteamWorkshop, out var modInfoData)) {
                Debug.LogError($"Failed to load mod {modPath}");
                return;
            }

            switch (modInfoData.version) {
                case "v0.0.0":
                    _ = instance.AddMod(new ModAddressable(modInfoData));
                    break;
                case "v0.0.1":
                    _ = instance.AddMod(new ModAssetBundle(modInfoData));
                    break;
                default:
                    Debug.LogError($"Failed to load mod {modPath}, unknown version {modInfoData.version}.");
                    return;
            }
        } catch (Exception e) {
            instance.lastException = e;
            Debug.LogException(e);
            Debug.LogError($"Failed to load mod {modPath}.");
        }
    }

    public static async Task RemoveMod(string modPath) {
        await Mutex.WaitAsync();
        try {
            DirectoryInfo directoryInfo = new DirectoryInfo(modPath);
            for (int i = 0; i < instance.fullModList.Count; i++) {
                if (instance.fullModList[i].info.directoryInfo.FullName == directoryInfo.FullName) {
                    instance.fullModList.RemoveAt(i);
                }
            }
        } finally {
            Mutex.Release();
            instance.modListChanged?.Invoke();
        }
    }
    private async Task AddMod(Mod mod) {
        if (!mod.IsValid()) {
            Debug.LogError($"{mod.info.title} [{mod.info.publishedFileId}] is not valid, can't load!");
            return;
        }
        if (mod.info.publishedFileId == (PublishedFileId_t)2934088282) {
            Debug.Log("Skipping surfmap, as its now included in the base game...");
            return;
        }
        await Mutex.WaitAsync();
        try {
            bool modFound = false;
            foreach (var search in fullModList) {
                if (search.info.source == ModSource.SteamWorkshop &&
                    mod.info.source == ModSource.SteamWorkshop &&
                    mod.info.publishedFileId != PublishedFileId_t.Invalid &&
                    search.info.publishedFileId == mod.info.publishedFileId) {
                    modFound = true;
                    break;
                }

                if (!search.IsValid()) {
                    if (!search.causedException) {
                        Debug.LogError($"{search.info.title} [{search.info.publishedFileId}] is not valid, can't load!");
                    }
                    search.causedException = true;
                    search.enabled = false;
                    continue;
                }

                if (mod.info.directoryInfo.FullName == search.info.directoryInfo.FullName) {
                    modFound = true;
                    break;
                }
            }

            if (modFound) {
                Debug.Log($"Already have mod {mod.info.title} [{mod.info.publishedFileId}], skipping...");
                return;
            }
            fullModList.Add(mod);
            fullModList.Sort(SortMods);
        } catch (Exception e) {
            Debug.LogException(e);
            lastException = e;
            mod.causedException = true;
            mod.enabled = false;
        } finally {
            Mutex.Release();
            modListChanged?.Invoke();
        }
    }

    private int SortMods(Mod a, Mod b) {
        if (a.info.loadPriority == b.info.loadPriority) {
            return String.Compare(a.info.title, b.info.title, StringComparison.InvariantCulture);
        }

        return a.info.loadPriority.CompareTo(b.info.loadPriority);
    }

    private void LoadConfig() {
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
                var modStub = new ModStub(node);
                foreach (var mod in instance.fullModList) {
                    if (!mod.GetRepresentedByStub(modStub)) continue;
                    mod.enabled = modStub.enabled;
                }
            } catch (Exception e) {
                instance.lastException = e;
                Debug.LogException(e);
                Debug.LogError($"Failed to load mod {node}.");
            }
        }

        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
        modListChanged?.Invoke();
        instance.changed = false;
    }
    private async Task ScanForNewMods() {
        status = ModStatus.ScanningForMods;
        string modCatalogPath = $"{Application.persistentDataPath}/{modLocation}";
        if (!Directory.Exists(modCatalogPath)) {
            Directory.CreateDirectory(modCatalogPath);
        }

        foreach (string directory in Directory.EnumerateDirectories(modCatalogPath)) {
            try {
                FileInfo infoPath = new FileInfo($"{directory}/info.json");
                if (!infoPath.Exists) {
                    Debug.LogError($"Failed to load mod {directory}, no info.json found.");
                    continue;
                }
                if (ModInfoData.TryGetModInfoData(infoPath.FullName, ModSource.LocalModFolder, out var data)) {
                    switch (data.version) {
                        case "v0.0.0": {
                            await AddMod(new ModAddressable(data));
                            break;
                        }
                        case "v0.0.1": {
                            await AddMod(new ModAssetBundle(data));
                            break;
                        }
                    }
                }
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
        instance.changed = false;
    }

    private async Task SyncEnabledStatusWithLoaded() {
        status = ModStatus.LoadingAssets;
        try {
            foreach(var mod in fullModList) {
                if (!mod.enabled) {
                    await mod.SetAssetsAvailable(false);
                }
            }
            foreach(var mod in fullModList) {
                if (mod.enabled) {
                    await mod.SetAssetsAvailable(true);
                }
            }
        } catch (Exception e) {
            Debug.LogException(e);
            failedToLoadMods = true;
            lastException = e;
            throw;
        } finally {
            ready = true;
            status = ModStatus.Ready;
            finishedLoading?.Invoke();
        }
    }

    public static bool GetReady() => GetFinishedLoading();
    public static ModStatus GetStatus() {
        return instance.status;
    }

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

    private static List<CancellationTokenSource> cancelTokenSources = new();
    private void HandleException(AsyncOperationHandle handle, Exception e) {
        for (int i=0;i<cancelTokenSources.Count;i++) {
            cancelTokenSources[i]?.Cancel();
        }
        lastException = e;
    }

    private void Start() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }
        status = ModStatus.Initializing;
        ResourceManager.ExceptionHandler += HandleException;
        Addressables.InternalIdTransformFunc += location => {
            if (location.InternalId.Contains("<currentLoadingMod>")) {
                return location.InternalId.Replace("<currentLoadingMod>", currentLoadingMod);
            }
            return location.InternalId;
        };

        ready = false;
        instance = this;
        fullModList = new List<Mod>();
        foreach(var modPostProcessor in earlyModPostProcessors) {
            modPostProcessor.Awake();
        }
        foreach(var modPostProcessor in modPostProcessors) {
            modPostProcessor.Awake();
        }

        OnStart();
    }

    private async Task OnStart() {
        await ScanForNewMods();
        status = ModStatus.WaitingForDownloads;
        while (SteamWorkshopModLoader.IsBusy) {
            await Task.Delay(1000);
        }
        LoadConfig();
        await SyncEnabledStatusWithLoaded();
        StringBuilder builder = new StringBuilder();
        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
        builder.Append("Mods Installed: {\n");
        foreach (var mod in GetFullModList()) {
            builder.Append($"[title:{mod.title}, folderTitle:{mod.folderTitle}, id:{mod.id}, source:{mod.source}, enabled:{mod.enabled}, causedException:{mod.causedException}],\n");
        }
        builder.Append("}\n");
        Debug.Log(builder.ToString());
    }
    
    public static List<ModStub> GetModsWithLoadedAssets() {
        return ConvertToStubs(instance.fullModList, (info) => info.enabled && info.GetAssetsLoaded());
    }

    public static async Task AllModsSetActive(bool active) {
        await Mutex.WaitAsync();
        try {
            foreach (var mod in instance.fullModList) {
                mod.enabled = active;
                instance.changed = true;
            }
        } finally {
            Mutex.Release();
        }
        await instance.SyncEnabledStatusWithLoaded();
        instance.playerConfig = ConvertToStubs(instance.fullModList, (info)=>info.enabled);
    }

    public static bool HasExactModConfigurationLoaded(IList<ModStub> stubs) {
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
                if (mod.info.title != stub.title || mod.info.publishedFileId != stub.id) continue;
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

    public static IEnumerator SetLoadedMods(IList<ModStub> stubs) {
        try {
            instance.status = ModStatus.Initializing;
            yield return new WaitUntil(GetReady);
            if (HasExactModConfigurationLoaded(stubs)) {
                instance.status = ModStatus.Ready;
                yield break;
            }

            instance.status = ModStatus.LoadingMods;
            Debug.Log("Loading mod stubs...");
            List<ModStub> neededMods = new List<ModStub>(stubs);
            for (int i = 0; i < neededMods.Count; i++) {
                if (neededMods[i].id != PublishedFileId_t.Invalid) {
                    continue;
                }

                foreach (var mod in instance.fullModList) {
                    if (mod.info.title != neededMods[i].title) continue;
                    neededMods.RemoveAt(i--);
                    break;
                }
            }

            // Now we have a list of needed mods
            foreach (var modStub in neededMods) {
                if (modStub.id == PublishedFileId_t.Invalid) {
                    throw new UnityException(
                        $"Couldn't find mod with name and id {modStub.title}, {modStub.id}. Can't continue! Tell the mod creator to upload it to Steam, or you need to manually install it.");
                }
            }

            foreach (var mod in instance.fullModList) {
                mod.enabled = false;
            }

            instance.changed = true;

            PublishedFileId_t[] fileIds = new PublishedFileId_t[neededMods.Count];
            for (int i = 0; i < neededMods.Count; i++) {
                fileIds[i] = neededMods[i].id;
            }

            instance.status = ModStatus.WaitingForDownloads;
            yield return SteamWorkshopModLoader.TryDownloadAllMods(fileIds);
            foreach (var modStub in stubs) {
                bool found = false;
                foreach (var mod in instance.fullModList) {
                    if (mod.info.title != modStub.title || mod.info.publishedFileId != modStub.id) continue;
                    found = true;
                    mod.enabled = true;
                    break;
                }

                if (!found) {
                    throw new UnityException(
                        $"Couldn't find mod with name and id {modStub.title}, {modStub.id}. Can't continue! It must have failed to download from the steam workshop, try logging into Steam!");
                }
            }

            Debug.Log("Reloading mods after acquiring stubs...");
            instance.status = ModStatus.LoadingAssets;
            var reloadModTask = instance.SyncEnabledStatusWithLoaded();
            yield return new WaitUntil(() => reloadModTask.IsCompleted);
            Debug.Log("Done reloading!");
        } finally {
            instance.status = ModStatus.Ready;
        }
    }
}
