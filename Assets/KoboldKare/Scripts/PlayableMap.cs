using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
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
    
    public string title;
    public Sprite preview;
    public string description;
    public AssetReferenceScene unityScene;

    private void OnValidate() {
#if UNITY_EDITOR
        if (unityScene.editorAsset == null) return;
        var serializedObject = new SerializedObject(this);
        serializedObject.FindProperty("unityScene").FindPropertyRelative("name").stringValue = unityScene.editorAsset.name;
        serializedObject.ApplyModifiedProperties();
#endif
    }
}
