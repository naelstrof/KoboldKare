#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Naelstrof {
    [CustomEditor(typeof(Naelstrof.Penetratable))]
    public class PenetratableEditor : Editor {
        int tab = 0;
        float testGirth = 0.05f;
        int testOffset = 0;
        List<int> pullBlendshapes = new List<int>();
        List<int> pushBlendshapes = new List<int>();
        List<int> expandBlendshapes = new List<int>();
        SerializedProperty holeForwardAxis;
        SerializedProperty holeUpAxis;
        SerializedProperty holeMeshes;
        SerializedProperty holeDiameter;
        SerializedProperty sampleOffset;
        SerializedProperty pullSampleOffset;
        SerializedProperty pushSampleOffset;
        SerializedProperty pullBlendshapeName;
        SerializedProperty pushBlendshapeName;
        SerializedProperty expandBlendshapeName;
        void OnEnable() {
            holeForwardAxis = serializedObject.FindProperty("holeForwardAxis");
            holeUpAxis = serializedObject.FindProperty("holeUpAxis");
            holeDiameter = serializedObject.FindProperty("holeDiameter");
            sampleOffset = serializedObject.FindProperty("sampleOffset");
            holeMeshes = serializedObject.FindProperty("holeMeshes");
            pullSampleOffset = serializedObject.FindProperty("pullSampleOffset");
            pushSampleOffset = serializedObject.FindProperty("pushSampleOffset");
            pullBlendshapeName = serializedObject.FindProperty("pullBlendshapeName");
            pushBlendshapeName = serializedObject.FindProperty("pushBlendshapeName");
            expandBlendshapeName = serializedObject.FindProperty("expandBlendshapeName");
        }
        string[] GetOptions(Mesh m) {
            List<string> options = new List<string>();
            options.Add("None");
            for (int i = 0; i < m.blendShapeCount; i++) {
                options.Add(m.GetBlendShapeName(i));
            }
            return options.ToArray();
        }
        int GetID(string name, Mesh m) {
            if (name == "") {
                return -1;
            }
            for (int i = 0; i < m.blendShapeCount; i++) {
                if (m.GetBlendShapeName(i) == name) {
                    return i;
                }
            }
            return -1;
        }
        string GetName(int id, Mesh m) {
            if (id == -1) {
                return "";
            }
            return m.GetBlendShapeName(id);
        }
        public override void OnInspectorGUI() {
            Naelstrof.Penetratable p = ((Naelstrof.Penetratable)serializedObject.targetObject);
            serializedObject.Update();
            Undo.RecordObject(serializedObject.targetObject, "Changes to " + serializedObject.targetObject.name);
            //EditorGUILayout.PropertyField(holeForwardAxis);
            //EditorGUILayout.PropertyField(holeUpAxis);
            p.holeRightAxis = Vector3.Cross(holeForwardAxis.vector3Value, holeUpAxis.vector3Value);
            //EditorGUILayout.PropertyField(holeTransform);
            EditorGUILayout.PropertyField(holeMeshes, true);
            bool notNull = true;
            for (int i = 0; i < p.holeMeshes.Count; i++) {
                notNull &= p.holeMeshes[i] != null;
            }
            if (p.holeMeshes != null && p.holeTransform != null && p.holeMeshes.Count > 0 && notNull) {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                int oldTab = tab;
                tab = GUILayout.Toolbar(tab, new string[] { "Hole Setup", "Preview" });
                if (oldTab == 1 && tab == 0 && expandBlendshapes.Count > 0) {
                    for (int i = 0; i < p.holeMeshes.Count; i++) {
                        if (expandBlendshapes[i] != -1) {
                            p.holeMeshes[i].SetBlendShapeWeight(expandBlendshapes[i], 0);
                        }
                        if (pushBlendshapes[i] != -1) {
                            p.holeMeshes[i].SetBlendShapeWeight(pushBlendshapes[i], 0f);
                        }
                        if (pullBlendshapes[i] != -1) {
                            p.holeMeshes[i].SetBlendShapeWeight(pullBlendshapes[i], 0f);
                        }
                    }
                }
                switch (tab) {
                    case 0:
                        string[] options = GetOptions(p.holeMeshes[0].sharedMesh);
                        pullBlendshapes.Clear();
                        pushBlendshapes.Clear();
                        expandBlendshapes.Clear();
                        for (int i = 0; i < p.holeMeshes.Count; i++) {
                            pullBlendshapes.Add(GetID(pullBlendshapeName.stringValue, p.holeMeshes[i].sharedMesh));
                            pushBlendshapes.Add(GetID(pushBlendshapeName.stringValue, p.holeMeshes[i].sharedMesh));
                            expandBlendshapes.Add(GetID(expandBlendshapeName.stringValue, p.holeMeshes[i].sharedMesh));
                        }
                        // Let them update the IDs
                        pullBlendshapes[0] = EditorGUILayout.Popup(new GUIContent("Pull Blendshape", "This blendshape is triggered when a penetrator is pulling out."), pullBlendshapes[0] + 1, options) - 1;
                        pushBlendshapes[0] = EditorGUILayout.Popup(new GUIContent("Push Blendshape", "This blendshape is triggered when a penetrator is pushing in."), pushBlendshapes[0] + 1, options) - 1;
                        expandBlendshapes[0] = EditorGUILayout.Popup(new GUIContent("Expand Blendshape", "This blendshape gets triggered based on the girth of the penetrator."), expandBlendshapes[0] + 1, options) - 1;

                        // Reupdate the blendshape string names.
                        pullBlendshapeName.stringValue = GetName(pullBlendshapes[0], p.holeMeshes[0].sharedMesh);
                        pushBlendshapeName.stringValue = GetName(pushBlendshapes[0], p.holeMeshes[0].sharedMesh);
                        expandBlendshapeName.stringValue = GetName(expandBlendshapes[0], p.holeMeshes[0].sharedMesh);

                        break;
                    case 1:
                        if (expandBlendshapes.Count == 0 || expandBlendshapes[0] == -1) {
                            EditorGUILayout.LabelField("There needs to be an expand blendshape set to test.");
                        } else {
                            testGirth = EditorGUILayout.Slider(new GUIContent("Test Girth", "The preview girth in scene, try to match the hole to the pink ring"), testGirth, 0f, 0.5f);
                            EditorGUILayout.HelpBox("Adjust the hole diameter slider until the hole roughly encompasses the circle in the view.", MessageType.Info);
                            EditorGUILayout.PropertyField(holeDiameter, new GUIContent("Hole Diameter", "The diameter of the hole when fully triggered in meters."));
                            EditorGUILayout.HelpBox("Adjust the sample offsets to align the circle to the hole in each state (neutral, pull, push)", MessageType.Info);
                            testOffset = GUILayout.Toolbar(testOffset, new string[] { "Neutral", "Push", "Pull" });
                            switch (testOffset) {
                                case 0:
                                    EditorGUILayout.PropertyField(sampleOffset, new GUIContent("Sample Offset", "Align the white circle to the hole with this slider in the 3D view."));
                                    break;
                                case 1:
                                    EditorGUILayout.PropertyField(pushSampleOffset, new GUIContent("Push Sample Offset", "Align the cyan circle to the hole with this slider in the 3D view."));
                                    break;
                                case 2:
                                    EditorGUILayout.PropertyField(pullSampleOffset, new GUIContent("Pull Sample Offset", "Align the magenta circle to the hole with this slider in the 3D view."));
                                    break;
                            }
                        }
                        break;
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                //DrawDefaultInspector();
                DrawPropertiesExcluding(serializedObject, "holeMeshes", "pullSampleOffset", "pushSampleOffset", "sampleOffset");
                //EditorGUILayout.PropertyField(dickTarget);
                //EditorGUILayout.PropertyField(closeSpring);
                //EditorGUILayout.PropertyField(moveSpring);
                //EditorGUILayout.PropertyField(moveDamping);
                //EditorGUILayout.PropertyField(concealsDick);
                //EditorGUILayout.PropertyField(OnPenetrate);
                //EditorGUILayout.PropertyField(OnDepenetrate);
                //EditorGUILayout.PropertyField(OnMove);
                //EditorGUILayout.PropertyField(connectedContainer);
                //EditorGUILayout.PropertyField(body);
            }
            serializedObject.ApplyModifiedProperties();
            if (p.holeMeshes.Count == 0 || p.holeTransform == null) {
                return;
            }
            if (tab == 1 && expandBlendshapes[0] != -1) {
                Vector3 forwardAxis = p.holeForwardAxis;
                Vector3 upAxis = p.holeUpAxis;
                Vector3 rightAxis = Vector3.Cross(forwardAxis, upAxis);
                for (int i = 0; i < p.holeMeshes.Count; i++) {
                    if (p.holeMeshes[i] == null) {
                        continue;
                    }
                    switch (testOffset) {
                        case 0:
                            if (pushBlendshapes[i] != -1) {
                                p.holeMeshes[i].SetBlendShapeWeight(pushBlendshapes[i], 0f);
                            }
                            if (pullBlendshapes[i] != -1) {
                                p.holeMeshes[i].SetBlendShapeWeight(pullBlendshapes[i], 0f);
                            }
                            break;
                        case 1:
                            if (pushBlendshapes[i] != -1) {
                                p.holeMeshes[i].SetBlendShapeWeight(pushBlendshapes[i], 100f);
                            }
                            if (pullBlendshapes[i] != -1) {
                                p.holeMeshes[i].SetBlendShapeWeight(pullBlendshapes[i], 0f);
                            }
                            break;
                        case 2:
                            if (pushBlendshapes[i] != -1) {
                                p.holeMeshes[i].SetBlendShapeWeight(pushBlendshapes[i], 0f);
                            }
                            if (pullBlendshapes[i] != -1) {
                                p.holeMeshes[i].SetBlendShapeWeight(pullBlendshapes[i], 100f);
                            }
                            break;
                    }
                    if (expandBlendshapes[i] != -1) {
                        p.holeMeshes[i].SetBlendShapeWeight(expandBlendshapes[i], (testGirth / p.GetWorldHoleDiameter()) * 100f);
                    }
                }
            }
        }
        public void OnSceneGUI() {
            if (Application.isPlaying) {
                return;
            }
            Naelstrof.Penetratable p = ((Naelstrof.Penetratable)serializedObject.targetObject);
            if (p.holeMeshes.Count == 0 || p.holeTransform == null) {
                return;
            }
            if (tab == 1 && expandBlendshapes[0] != -1) {
                Vector3 forwardAxis = p.holeForwardAxis;
                Vector3 upAxis = p.holeUpAxis;
                Vector3 rightAxis = Vector3.Cross(forwardAxis, upAxis);
                Handles.color = new Color(1, 1, 1, testOffset == 0 ? 1 : 0.1f);
                Handles.DrawWireDisc(p.holeTransform.TransformPoint(p.sampleOffset * p.holeForwardAxis), p.holeTransform.TransformDirection(forwardAxis), testGirth);
                Color mag = Color.magenta;
                mag.a = testOffset == 2 ? 1 : 0.1f;
                Handles.color = mag;
                Handles.DrawWireDisc(p.holeTransform.TransformPoint(p.pullSampleOffset * p.holeForwardAxis), p.holeTransform.TransformDirection(forwardAxis), testGirth);
                Color cyan = Color.cyan;
                cyan.a = testOffset == 1 ? 1 : 0.1f;
                Handles.color = cyan;
                Handles.DrawWireDisc(p.holeTransform.TransformPoint(p.pushSampleOffset * p.holeForwardAxis), p.holeTransform.TransformDirection(forwardAxis), testGirth);
            }
        }
    }
}
#endif
