using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ModManager : MonoBehaviour {
    private Dictionary<string,object> loadedAssets = new Dictionary<string, object>();
    public static async Task GetAll(string label, IList<IResourceLocation> loadedLocations) {
        var unloadedLocations = await Addressables.LoadResourceLocationsAsync(label).Task;
        foreach(var location in unloadedLocations) {
            loadedLocations.Add(location);
        }
    }
    private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<object> Load(string key) {
        var t = Addressables.LoadAssetAsync<object>(key);
        t.Completed += (asset) => {
            if (loadedAssets.ContainsKey(key)) { Addressables.Release(loadedAssets[key]); }
            loadedAssets[key] = asset;
        };
        return t;
    }
    public async void LoadAtStart() {
        List<IResourceLocation> resources = new List<IResourceLocation>();
        await GetAll("LoadAtStart", resources);
        foreach(var resource in resources) {
            Load(resource.PrimaryKey);
        }
    }
    public void Awake() {
        LoadAtStart();
    }
    public void OnDestroy() {
        foreach(var pair in loadedAssets) {
            Addressables.Release(loadedAssets[pair.Key]);
        }
    }
}
