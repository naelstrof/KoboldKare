using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(ScriptableFloatSlider))]
public class ScriptableFloatSliderEditor : SliderEditor {
    public SerializedProperty val;
    public new void OnEnable() {
        val = serializedObject.FindProperty("val");
        base.OnEnable();
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(val);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
