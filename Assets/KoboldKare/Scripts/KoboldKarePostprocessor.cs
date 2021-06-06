using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class KoboldKarePostprocessor : AssetPostprocessor {
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string str in importedAssets) {
            System.Type type = AssetDatabase.GetMainAssetTypeAtPath(str);
            if (type.BaseType == typeof(PhysicsAudioGroup) ||
                type.BaseType == typeof(Equipment) ||
                type.BaseType == typeof(StatusEffect)) {
                Object obj = AssetDatabase.LoadAssetAtPath<Object>(str);
                bool isPreloaded = false;
                foreach (var preloaded in PlayerSettings.GetPreloadedAssets()) {
                    if (preloaded == obj) {
                        isPreloaded = true;
                    }
                }
                if (!isPreloaded) {
                    List<Object> newPreload = new List<Object>(PlayerSettings.GetPreloadedAssets());
                    newPreload.Add(obj);
                    PlayerSettings.SetPreloadedAssets(newPreload.ToArray());
                    Debug.Log("New " + type + " Asset detected, and added to PreloadedAssets: " + str);
                }
            }
        }
        if (deletedAssets.Length > 0) {
            bool removed = false;
            List<Object> newPreload = new List<Object>(PlayerSettings.GetPreloadedAssets());
            for(int i=0;i<newPreload.Count;i++) {
                if (newPreload[i] == null) {
                    newPreload.RemoveAt(i);
                    removed = true;
                }
            }
            if (removed) {
                Debug.Log("One or more preloaded asset was deleted, and was removed from PreloadedAssets list.");
                PlayerSettings.SetPreloadedAssets(newPreload.ToArray());
            }
        }

        //for (int i = 0; i < movedAssets.Length; i++) {
            //Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        //}
    }
}
#endif
