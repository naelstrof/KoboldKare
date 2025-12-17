using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class KoboldKareObjectPostProcessor : ModPostProcessor {
    [SerializeField] protected AssetLabelReference[] assetLabels;
    private static Dictionary<string, AssetGroup> assetDatabases;
    
    public static async Task<AssetGroup.AssetLocation.AssetHandle<T>> GetAssetAsync<T>(string group, string assetName, T missingResult) where T : Object {
        try {
            while (!ModManager.GetReady()) {
                await Task.Delay(1000);
            }

            if (assetDatabases != null && assetDatabases.ContainsKey(group)) {
                var database = assetDatabases[group];
                if (database.TryGetAssetLocation(assetName, out var assetLocation)) {
                    return await assetLocation.GetAssetAsync<T>();
                }

                Debug.LogWarning($"Asset {assetName} in group {group} is not found.");
                return new AssetGroup.AssetLocation.AssetHandle<T>(missingResult, null);
            }

            Debug.LogWarning($"Asset group {group} is not found.");
            return new AssetGroup.AssetLocation.AssetHandle<T>(missingResult, null);
        } catch (Exception e) {
            Debug.LogException(e);
            throw;
        }
    }
    
    public static async Task<AssetGroup.AssetLocation.AssetHandle<T>> GetAssetAsync<T>(string group, int id, T missingResult) where T : Object {
        try {
            if (assetDatabases != null && assetDatabases.ContainsKey(group)) {
                var database = assetDatabases[group];
                if (database.TryGetAssetLocation(id, out var assetLocation)) {
                    return await assetLocation.GetAssetAsync<T>();
                }

                Debug.LogWarning($"Asset ID {id} in group {group} is not found.");
                return new AssetGroup.AssetLocation.AssetHandle<T>(missingResult, null);
            }

            Debug.LogWarning($"Asset group {group} is not found.");
            return new AssetGroup.AssetLocation.AssetHandle<T>(missingResult, null);
        } catch (Exception e) {
            Debug.LogException(e);
            throw;
        }
    }

    public static bool GetAssetGroupFromKey(string key, out string group) {
        if (assetDatabases != null) {
            foreach (var databaseKVP in assetDatabases) {
                var database = databaseKVP.Value;
                if (database.ContainsKey(key)) {
                    group = databaseKVP.Key;
                    return true;
                }
            }
        }
        group = "";
        return false;
    }

    public static bool TryGetRandomAssetKey(string group, out string assetKey) {
        float range = 0f;
        if (assetDatabases != null && assetDatabases.ContainsKey(group)) {
            var database = assetDatabases[group];
            foreach (var asset in database.assets) {
                range += 1f;
            }
            float roll = Random.Range(0f, range);
            float cumulative = 0f;
            foreach (var asset in database.assets) {
                cumulative += 1f;
                if (roll <= cumulative) {
                    assetKey = asset.key;
                    return true;
                }
            }
        }
        assetKey = "";
        return false;
    }

    public static int GetAssetID(string group, string assetName) {
        if (assetDatabases != null && assetDatabases.ContainsKey(group)) {
            var database = assetDatabases[group];
            return database.GetAssetID(assetName);
        }
        return -1;
    }
    
    public static bool HasAssetInGroup(string group, string assetName) {
        if (assetDatabases != null && assetDatabases.ContainsKey(group)) {
            return true;
        }
        return false;
    }
    
    public static void GetAllAssetNamesInGroup(string group, List<string> output) {
        output.Clear();
        if (assetDatabases != null && assetDatabases.ContainsKey(group)) {
            var database = assetDatabases[group];
            database.GetAssetKeys(output);
        }
    }
    
    private static string GetCachedKeyFileLocation(string uniqueModID) {
        if (!Directory.Exists($"{Application.persistentDataPath}/modcache/")) {
            Directory.CreateDirectory($"{Application.persistentDataPath}/modcache/");
        }
        return $"{Application.persistentDataPath}/modcache/{uniqueModID}";
    }

    private bool TryGetCachedKeys(string uniqueModID, out JSONNode keys) {
        try {
            FileInfo fileInfo = new FileInfo(GetCachedKeyFileLocation(uniqueModID));
            if (fileInfo.Exists) {
                string fileContents = File.ReadAllText(fileInfo.FullName);
                keys = JSON.Parse(fileContents);
                return true;
            }
        } catch {
            Debug.LogWarning($"Failed to read cached mod keys with {uniqueModID}.");
            keys = null;
            return false;
        }
        keys = null;
        return false;
    }

    private void WriteCachedKeys(string uniqueModID, JSONNode keys) {
        FileInfo fileInfo = new FileInfo(GetCachedKeyFileLocation(uniqueModID));
        File.WriteAllText(fileInfo.FullName, keys.ToString());
    }

    public override async Task Awake() {
        assetDatabases = new Dictionary<string, AssetGroup>();
        await base.Awake();
        foreach (var label in assetLabels) {
            var handle = Addressables.LoadResourceLocationsAsync(label.RuntimeKey);
            await handle.Task;
            if (!assetDatabases.ContainsKey(label.labelString)) {
                assetDatabases[label.labelString] = new AssetGroup();
            }
            var database = assetDatabases[label.labelString];
            foreach (var location in handle.Result) {
                var nameWithoutPath = Path.GetFileNameWithoutExtension(location.PrimaryKey);
                database.AddAsset(nameWithoutPath, null, location.PrimaryKey);
            }
        }
    }

    public override Task HandleAssetBundleMod(ModManager.ModInfoData data, AssetBundle assetBundle) {
        var rootNode = data.assets;
        Debug.Log(data.title);
        foreach (var label in assetLabels) {
            if (rootNode.HasKey(label.labelString)) {
                if (!assetDatabases.ContainsKey(label.labelString)) {
                    assetDatabases[label.labelString] = new AssetGroup();
                }
                var array = rootNode[label.labelString].AsArray;
                foreach (var node in array) {
                    if (!node.Value.IsString) continue;
                    var assetName = node.Value;
                    var nameWithoutPath = Path.GetFileNameWithoutExtension(assetName);
                    assetDatabases[label.labelString].AddAsset(nameWithoutPath, new ModManager.ModStub(data), null);
                }
            }
        }
        return Task.CompletedTask;
    }
    

    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (!TryGetCachedKeys(data.publishedFileId.ToString(), out var keys)) {
            keys = JSONNode.Parse("{}");
            foreach (var label in assetLabels) {
                if (locator.Locate(label.RuntimeKey, typeof(Object), out var locations)) {
                    JSONNode labelKeys = JSONNode.Parse("{}");
                    foreach (var location in locations) {
                        var opHandle = Addressables.LoadAssetAsync<Object>(location);
                        await opHandle.Task;
                        labelKeys[opHandle.Result.name] = location.PrimaryKey;
                        Addressables.Release(opHandle);
                    }
                    keys[label.labelString] = labelKeys;
                }
            }
            WriteCachedKeys(data.publishedFileId.ToString(), keys);
        }
        foreach (var keyset in keys) {
            if (!assetDatabases.ContainsKey(keyset.Key)) {
                assetDatabases[keyset.Key] = new AssetGroup();
            }
            var database = assetDatabases[keyset.Key];
            foreach(var assetNameKeyPair in keyset.Value) {
                database.AddAsset(assetNameKeyPair.Key, new ModManager.ModStub(data), assetNameKeyPair.Value);
            }
        }
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        foreach (var assetDatabase in assetDatabases) {
            assetDatabase.Value.RemoveAllRepresentedByStub(new ModManager.ModStub(data));
        }
        return base.UnloadAssets(data);
    }
}
