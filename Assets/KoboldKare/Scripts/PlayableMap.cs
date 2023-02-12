using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;


[CreateAssetMenu(fileName = "New PlayableMap", menuName = "Data/PlayableMap")]
public class PlayableMap : ScriptableObject {
    [System.Serializable]
    public class AssetReferenceScene : AssetReference {
        public override bool ValidateAsset(Object obj) {
#if UNITY_EDITOR
            var type = obj.GetType();
            return typeof(UnityEditor.SceneAsset).IsAssignableFrom(type);
#else
            return false;
#endif
        }
        public override bool ValidateAsset(string path) {
#if UNITY_EDITOR
            var type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
            return typeof(UnityEditor.SceneAsset).IsAssignableFrom(type);
#else
            return false;
#endif
        }
    }
    
    public string title;
    public Sprite preview;
    public string description;
    public AssetReferenceScene unityScene;
}
