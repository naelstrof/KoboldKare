using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif


[CreateAssetMenu(fileName = "New PlayableMap", menuName = "Data/PlayableMap")]
public class PlayableMap : ScriptableObject {
    [System.Serializable]
    public class AssetReferenceScene : AssetReference {
        [SerializeField]
        private string name;
        public string GetName() => name;
        public override bool ValidateAsset(Object obj) {
#if UNITY_EDITOR
            var type = obj.GetType();
            return typeof(SceneAsset).IsAssignableFrom(type);
#else
            return false;
#endif
        }
        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return typeof(SceneAsset).IsAssignableFrom(type);
#else
            return false;
#endif
        }
    }
    
    [SerializeField] private string title;
    [SerializeField] private Sprite preview;
    [SerializeField] private string description;
    [SerializeField] private AssetReferenceScene unityScene;
    
    [SerializeField] private string bundlePath;
    [SerializeField] private string bundleAssetName;

    [NonSerialized]
    public ModManager.ModStub? stub;
    
    private struct ModStubAddressableHandlePair {
        public ModManager.ModStub stub;
        public AsyncOperationHandle handle;
    }
    [NonSerialized]
    private List<ModStubAddressableHandlePair> loadedHandles;
    
    private struct ModStubBundleHandlePair {
        public ModManager.ModStub stub;
        public AssetBundle bundle;
    }
    [NonSerialized]
    private List<ModStubBundleHandlePair> loadedBundles;

    public void SetPreview(Sprite preview) {
        this.preview = preview;
    }

    public void SetFromBundle(string path, string assetName, string title, Sprite preview, string description) {
        bundlePath = path;
        bundleAssetName = assetName;
        this.title = title;
        this.preview = preview;
        this.description = description;
    }

    public string GetTitle() => title;
    public Sprite GetPreview() => preview;
    public string GetDescription() => description;

    private void Awake() {
        loadedHandles = new();
        loadedBundles = new();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene arg0) {
        foreach(var handle in loadedHandles) {
            Addressables.Release(handle.handle);
            _ = ModManager.SetModAssetsAvailable(handle.stub, false);
        }
        loadedHandles.Clear();
        foreach(var bundle in loadedBundles) {
            bundle.bundle.Unload(true);
            _ = ModManager.SetModAssetsAvailable(bundle.stub, false);
        }
        loadedBundles.Clear();
    }


    public bool GetRepresentedByKey(string key) {
        if (string.IsNullOrEmpty(bundleAssetName)) {
            return key == (string)unityScene.RuntimeKey;
        } else {
            return key == GetSceneName();
        }
    }
    public string GetSceneName() {
        if (string.IsNullOrEmpty(bundleAssetName)) {
            return unityScene.GetName();
        } else {
            return Path.GetFileNameWithoutExtension(bundleAssetName);
        }
    }

    public string GetKey() {
        if (string.IsNullOrEmpty(bundleAssetName)) {
            return (string)unityScene.RuntimeKey;
        } else {
            return GetSceneName();
        }
    }

    private async Task LoadAddressableSceneFromStub(ModManager.ModStub? stub, object key) {
        await PlayableMapPostProcessor.UnloadAllMaps();
        if (stub != null) {
            await ModManager.SetModAssetsAvailable(stub.Value, true);
        }
        var handle = Addressables.LoadSceneAsync(key);
        await handle.Task;
        if (stub != null) {
            loadedHandles.Add(new ModStubAddressableHandlePair() {
                handle = handle,
                stub = stub.Value
            });
        }
    }

    private async Task LoadBundleSceneFromStub(ModManager.ModStub stub) {
        await PlayableMapPostProcessor.UnloadAllMaps();
        await ModManager.SetModAssetsAvailable(stub, true);
        var bundle = AssetBundle.LoadFromFile(bundlePath);
        var handle = SceneManager.LoadSceneAsync(GetSceneName(), LoadSceneMode.Single);
        if (handle == null) {
            Debug.LogError("Failed to load bundled scene: " + GetSceneName());
            return;
        }
        await handle.AsTask();
        loadedBundles.Add(new ModStubBundleHandlePair() {
            bundle = bundle,
            stub = stub
        });
    }

    public BoxedSceneLoad LoadAsync() {
        if (string.IsNullOrEmpty(bundleAssetName)) {
            return BoxedSceneLoad.FromTask(LoadAddressableSceneFromStub(stub, unityScene.RuntimeKey));
        } else {
            if (stub != null) {
                return BoxedSceneLoad.FromTask(LoadBundleSceneFromStub(stub.Value));
            } else {
                Debug.LogError("Failed to load bundled scene, there was no mod associated??: " + GetSceneName());
                return new BoxedSceneLoad();
            }
        }
    }

    private void OnValidate() {
#if UNITY_EDITOR
        if (unityScene.editorAsset == null) return;
        var serializedObject = new SerializedObject(this);
        serializedObject.FindProperty("unityScene").FindPropertyRelative("name").stringValue = unityScene.editorAsset.name;
        serializedObject.ApplyModifiedProperties();
#endif
    }
}
