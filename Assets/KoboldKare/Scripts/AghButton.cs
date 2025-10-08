using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public static class AghButton {
    [MenuItem("Tools/KoboldKare/Unhide GameObjects")]
    public static void UnhideGameObjects() {
        foreach (GameObject g in Object.FindObjectsOfType<GameObject>(true)) {
            g.hideFlags &= (HideFlags)(~0x3);
        }
    }

    [MenuItem("Tools/KoboldKare/Prefabify Button")]
    public static void PrefabifyButton() {
        var prefabGUID = "f7a9775ff8c444f3c964c30b914805d0";
        var path = AssetDatabase.GUIDToAssetPath(prefabGUID);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        
        foreach (GameObject g in Selection.gameObjects) {
            foreach (Button b in g.GetComponentsInChildren<Button>(true)) {
                GameObject newButtonObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, b.transform.parent);
                newButtonObj.transform.position = b.transform.position;
                newButtonObj.transform.rotation = b.transform.rotation;
                newButtonObj.transform.localScale = b.transform.localScale;
                newButtonObj.name = b.gameObject.name;
                Undo.RegisterCreatedObjectUndo(newButtonObj, "Created prefab button");
                var newButton = newButtonObj.GetComponent<Button>();
                newButton.onClick = b.onClick;

                foreach (var comp in b.gameObject.GetComponents<Component>()) {
                    if (comp is RectTransform || comp is Image || comp is Button || comp is TMP_Text || comp is LocalizeStringEvent || comp is TextRTLFixer || comp is ButtonMouseOver) {
                        continue;
                    }
                    UnityEditorInternal.ComponentUtility.CopyComponent(comp);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(newButtonObj);
                }
                
                var text = b.GetComponentInChildren<TMP_Text>();
                var otherText = newButtonObj.GetComponentInChildren<TMP_Text>();
                otherText.text = text.text;
                otherText.fontSize = text.fontSize;
                otherText.alignment = text.alignment;
                otherText.color = text.color;
                otherText.font = text.font;
                otherText.enableWordWrapping = text.enableWordWrapping;
                otherText.raycastTarget = text.raycastTarget;
                otherText.fontStyle = text.fontStyle;
                otherText.characterSpacing = text.characterSpacing;
                otherText.lineSpacing = text.lineSpacing;
                otherText.enableAutoSizing = text.enableAutoSizing;
                var localizeStringEvent = b.GetComponentInChildren<LocalizeStringEvent>();
                var otherLocalizeStringEvent = newButtonObj.GetComponentInChildren<LocalizeStringEvent>();
                if (localizeStringEvent) {
                    otherLocalizeStringEvent.StringReference = localizeStringEvent.StringReference;
                } else {
                    otherLocalizeStringEvent.StringReference = null;
                }

                var iconGameObj = b.transform.Find("Icon");
                if (iconGameObj != null) {
                    var icon = iconGameObj.GetComponent<Image>();
                    var otherIcon = newButtonObj.transform.Find("Icon").GetComponent<Image>();
                    otherIcon.sprite = icon.sprite;
                    otherIcon.color = icon.color;
                }
            }
        }
    }

    [MenuItem("Tools/KoboldKare/FindInvalidTransforms")]
    public static void FindInvalidTransform() {
        foreach (GameObject g in Object.FindObjectsOfType<GameObject>(true)) {
            Quaternion checkQuat = g.transform.rotation;
            Vector3 checkPos = g.transform.position;
            if (float.IsNaN(checkQuat.x) ||
                float.IsNaN(checkQuat.y) ||
                float.IsNaN(checkQuat.z) ||
                float.IsNaN(checkQuat.w) ||
                float.IsNaN(checkPos.x) ||
                float.IsNaN(checkPos.y) ||
                float.IsNaN(checkPos.z)) {
                Selection.activeObject = g;
                Debug.Log("Found invalid transform!", g);
                return;
            }
        }

        Debug.Log("None found!");
    }

    private static int RTLFix(TMP_Text t) {
        if (t.GetComponent<TextRTLFixer>() != null) {
            return 0;
        }
        Undo.RecordObject(t.gameObject, "added component");
        t.gameObject.AddComponent<TextRTLFixer>();
        EditorUtility.SetDirty(t.gameObject);
        return 1;
    }

    //[MenuItem("Tools/KoboldKare/Text RTL Fixer")]
    public static void RTLFixer() {
        int fixes = 0;
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Added components");
        var undoIndex = Undo.GetCurrentGroup();
        foreach(GameObject g in Selection.gameObjects) {
            foreach(TMP_Text t in g.GetComponentsInChildren<TMP_Text>(true)) {
                fixes += RTLFix(t);
            }
        }
        foreach (var g in Object.FindObjectsOfType<GameObject>()) {
            foreach (TMP_Text t in g.GetComponentsInChildren<TMP_Text>(true)) {
                fixes += RTLFix(t);
            }
        }
        string[] pathsToAssets = AssetDatabase.FindAssets("t:GameObject");
        foreach (var path in pathsToAssets) {
            var path1 = AssetDatabase.GUIDToAssetPath(path);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path1);
            foreach(TMP_Text t in go.GetComponentsInChildren<TMP_Text>(true)) {
                if (t.GetComponent<TextRTLFixer>() == null) {
                    Selection.activeGameObject = go;
                    Debug.Log($"Fixed {fixes} text mesh pros, need assistance, open this prefab and run again.");
                    return;
                }
                //fixes += RTLFix(t);
            }
        }
        Undo.CollapseUndoOperations(undoIndex);
        Debug.Log($"Fixed {fixes} text mesh pros, none found left!");
    }

    //[MenuItem("Tools/KoboldKare/HomogenizeButtons")]
    public static void HomogenizeButtons() {
        ColorBlock block = new ColorBlock();
        block.normalColor = Color.white;
        block.colorMultiplier = 1f;
        block.highlightedColor = new Color(0.49f,1f,0.9435571f, 1f);
        block.pressedColor = new Color(0.1090246f, 0.4716981f, 0.7129337f, 1f);
        block.selectedColor = new Color(0.2559185f, 0.764151f, 0.7129337f, 1f);
        block.disabledColor = new Color(0.7843137f, 0.7843137f, 0.7843137f, 0.5019608f);
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Changed button colors");
        var undoIndex = Undo.GetCurrentGroup();
        foreach(GameObject g in Selection.gameObjects) {
            foreach(Button b in g.GetComponentsInChildren<Button>(true)) {
                Undo.RecordObject(b, "Changed button color");
                b.colors = block;
                EditorUtility.SetDirty(b);
            }

            foreach (Toggle t in g.GetComponentsInChildren<Toggle>(true)) {
                Undo.RecordObject(t, "Changed button color");
                t.colors = block;
                EditorUtility.SetDirty(t);
            }
        }
        Undo.CollapseUndoOperations(undoIndex);
    }
    //[MenuItem("Tools/KoboldKare/Disable all GPU Instanced Materials")]
    public static void FindGPUInstancedMaterial() {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Changed project gpu instancing");
        var undoIndex = Undo.GetCurrentGroup();
        string[] pathsToAssets = AssetDatabase.FindAssets("t:Material");
        foreach (var path in pathsToAssets) {
            var path1 = AssetDatabase.GUIDToAssetPath(path);
            var go = AssetDatabase.LoadAssetAtPath<Material>(path1);
            if (go.enableInstancing) {
                Undo.RecordObject(go, "Changed project gpu instancing");
                go.enableInstancing = false;
                EditorUtility.SetDirty(go);
            }
        }
        Undo.CollapseUndoOperations(undoIndex);
    }
    //[MenuItem("Tools/KoboldKare/Find Specular Workflow Materials")]
    public static void FindSpecularWorkflowMaterials() {
        string[] pathsToAssets = AssetDatabase.FindAssets("t:Material");
        foreach (var path in pathsToAssets) {
            var path1 = AssetDatabase.GUIDToAssetPath(path);
            var go = AssetDatabase.LoadAssetAtPath<Material>(path1);
            if (go.IsKeywordEnabled("_SPECULAR_SETUP")) {
                Selection.activeObject = go;
                return;
            }
        }
    }
    //[MenuItem("Tools/KoboldKare/Enable all environment reflections, specular highlights (to reduce shader variants.)")]
    public static void FindSpecularOffMaterials() {
        string[] pathsToAssets = AssetDatabase.FindAssets("t:Material");
        foreach (var path in pathsToAssets) {
            var path1 = AssetDatabase.GUIDToAssetPath(path);
            var check = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path1);
            if (check != null && check.source != PackageSource.Local && check.source != PackageSource.Embedded) {
                continue;
            }

            string reason = "";
            var go = AssetDatabase.LoadAssetAtPath<Material>(path1);
            if (go.IsKeywordEnabled("_SPECULARHIGHTLIGHTS_OFF")) {
                go.DisableKeyword("_SPECULARHIGHTLIGHTS_OFF");
                reason = "_SPECULARHIGHTLIGHTS_OFF";
            }
            if (go.IsKeywordEnabled("_RECEIVE_SHADOWS_OFF")) {
                go.DisableKeyword("_RECEIVE_SHADOWS_OFF");
                reason = "_RECEIVE_SHADOWS_OFF";
            }
            if (go.IsKeywordEnabled("_ENVIRONMENTREFLECTIONS_OFF")) {
                go.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                reason = "_ENVIRONMENTREFLECTIONS_OFF";
            }
            if (go.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON")) {
                go.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                reason = "_ALPHAPREMULTIPLY_ON";
            }
            if (go.IsKeywordEnabled("_OCCLUSIONMAP")) {
                go.DisableKeyword("_OCCLUSIONMAP");
                reason = "_OCCLUSIONMAP";
            }
            if (go.IsKeywordEnabled("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A")) {
                go.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                reason = "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A";
            }

            if (!string.IsNullOrEmpty(reason)) {
                EditorUtility.SetDirty(go);
                Debug.Log($"Selecting {go} because {reason}");
                Selection.activeObject = go;
                return;
            }
        }

        Debug.Log("No found errornous materials.");
    }
    //[MenuItem("Tools/KoboldKare/Find Spacialized Audio")]
    public static void FindSpacializedAudio() {
        int count = 0;
        foreach(GameObject g in Selection.gameObjects) {
            foreach(var c in g.GetComponents<AudioSource>()) {
                if (c.spatialize) {
                    c.spatialize = false;
                    count++;
                    Selection.activeGameObject = g;
                    EditorUtility.SetDirty(g);
                    EditorUtility.SetDirty(c);
                }
            }
        }
        foreach(var g in Object.FindObjectsOfType<GameObject>()) {
            foreach(var c in g.GetComponents<AudioSource>()) {
                if (c.spatialize) {
                    c.spatialize = false;
                    count++;
                    Selection.activeGameObject = g;
                    EditorUtility.SetDirty(g);
                    EditorUtility.SetDirty(c);
                }
            }
        }
        string[] pathsToAssets = AssetDatabase.FindAssets("t:GameObject");
        foreach (var path in pathsToAssets) {
            var path1 = AssetDatabase.GUIDToAssetPath(path);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path1);
            foreach(var c in go.GetComponentsInChildren<AudioSource>(true)) {
                if (c.spatialize) {
                    c.spatialize = false;
                    count++;
                    Selection.activeGameObject = go;
                }
            }
        }
        Debug.Log("Found and attempted to fix " + count + " audio sources.");
    }
    [MenuItem("Tools/KoboldKare/Find Missing Script")]
    public static void FindMissingScript() {
        foreach(GameObject g in Selection.gameObjects) {
            foreach(var c in g.GetComponents<Component>()) {
                if (c == null) {
                    if (g.hideFlags == HideFlags.HideAndDontSave || g.hideFlags == HideFlags.HideInHierarchy || g.hideFlags == HideFlags.HideInInspector) {
                        Debug.Log("Found hidden gameobject with a missing script, deleted " + g);
                        GameObject.DestroyImmediate(g);
                        continue;
                    }
                    Selection.activeGameObject = g;
                    return;
                }
            }
        }
        foreach(var g in Object.FindObjectsOfType<GameObject>()) {
            foreach(var c in g.GetComponents<Component>()) {
                if (c == null) {
                    if (g.hideFlags == HideFlags.HideAndDontSave || g.hideFlags == HideFlags.HideInHierarchy || g.hideFlags == HideFlags.HideInInspector) {
                        Debug.Log("Found hidden gameobject with a missing script, deleted " + g);
                        GameObject.DestroyImmediate(g);
                        continue;
                    }
                    Selection.activeGameObject = g;
                    return;
                }
            }
        }
        string[] pathsToAssets = AssetDatabase.FindAssets("t:GameObject");
        foreach (var path in pathsToAssets) {
            var path1 = AssetDatabase.GUIDToAssetPath(path);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path1);
            foreach(var c in go.GetComponentsInChildren<Component>(true)) {
                if (c == null) {
                    Selection.activeGameObject = go;
                    return;
                }
            }
        }
        Debug.Log("No missing scripts found anywhere! Good job.");
    }
}

#endif