using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
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
    
    
    public bool GetRepresentedByKey(string key) {
        if (unityScene.IsValid()) {
            return key == (string)unityScene.RuntimeKey;
        } else {
            return key == GetSceneName();
        }
    }
    public string GetSceneName() {
        if (unityScene.IsValid()) {
            return unityScene.GetName();
        } else {
            return Path.GetFileNameWithoutExtension(bundleAssetName);
        }
    }

    public string GetKey() {
        if (unityScene.IsValid()) {
            return (string)unityScene.RuntimeKey;
        } else {
            return GetSceneName();
        }
    }

    public BoxedSceneLoad LoadAsync() {
        if (unityScene.IsValid()) {
            return BoxedSceneLoad.FromAddressables(Addressables.LoadSceneAsync(unityScene.RuntimeKey, LoadSceneMode.Single));
        } else {
            var bundle = AssetBundle.LoadFromFile(bundlePath);
            var handle = SceneManager.LoadSceneAsync(GetSceneName(), LoadSceneMode.Single);
            handle.completed += operation => {
                bundle.Unload(false);
            };
            return BoxedSceneLoad.FromUnity(handle);
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
