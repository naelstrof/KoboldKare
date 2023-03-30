using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CloudPool : GenericPool<Cloud> {
#if UNITY_EDITOR
    private void OnValidate() {
        if (prefab == null) {
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                AssetDatabase.GUIDToAssetPath("eecff7b2cef7a554fa2e0ce739b133a3"));
        }
    }
#endif
}
