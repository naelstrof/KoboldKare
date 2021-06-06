#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Naelstrof {
    [CustomEditor(typeof(Naelstrof.Dick))]
    public class DickEditor : Editor {
        int tab = 0;
        int previewID = 0;
        int previewDeformation = 0;
        float previewPlaneSlider = 99f;
        Naelstrof.Dick.BlendshapeType generateType = Naelstrof.Dick.BlendshapeType.Squish;
        bool generateAll = true;
        int bakeStatus = 0;
        int crossSectionCount = 16;
        SerializedProperty dickForwardAxis;
        SerializedProperty dickUpAxis;
        SerializedProperty dickTransform;
        SerializedProperty bakeMeshes;
        SerializedProperty deformationTargets;
        SerializedProperty blendshapeIDs;
        SerializedProperty girthCurves;
        SerializedProperty xOffsetCurves;
        SerializedProperty yOffsetCurves;
        SerializedProperty blendshapeSoftness;
        void OnEnable() {
            dickForwardAxis = serializedObject.FindProperty("dickForwardAxis");
            dickUpAxis = serializedObject.FindProperty("dickUpAxis");
            dickTransform = serializedObject.FindProperty("dickTransform");
            bakeMeshes = serializedObject.FindProperty("bakeMeshes");
            deformationTargets = serializedObject.FindProperty("deformationTargets");
            blendshapeIDs = serializedObject.FindProperty("blendshapeIDs");
            girthCurves = serializedObject.FindProperty("girthCurves");
            xOffsetCurves = serializedObject.FindProperty("xOffsetCurves");
            yOffsetCurves = serializedObject.FindProperty("yOffsetCurves");
            blendshapeSoftness = serializedObject.FindProperty("blendshapeSoftness");
        }
        public static void ApplyBlendshape(List<Vector3> verts, Mesh m, int id) {
            Vector3[] blendVerts = new Vector3[m.vertexCount];
            Vector3[] blendNormals = new Vector3[m.vertexCount];
            Vector3[] blendTangents = new Vector3[m.vertexCount];
            m.GetBlendShapeFrameVertices(id, 0, blendVerts, blendNormals, blendTangents);
            for (int i = 0; i < verts.Count; i++) {
                verts[i] += blendVerts[i];
            }
        }
        public static void SmoothCurve(AnimationCurve curve) {
            for (int i = 0; i < curve.keys.Length; ++i) {
                curve.SmoothTangents(i, 0);
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
        }
        void DrawApprox(Naelstrof.Dick.BlendshapeType type, int Samples) {
            Naelstrof.Dick d = ((Naelstrof.Dick)serializedObject.targetObject);
            Vector3 forward = dickForwardAxis.vector3Value;
            Vector3 up = dickUpAxis.vector3Value;
            Vector3 right = Vector3.Cross(forward, up);
            List<Keyframe> keys = new List<Keyframe>(d.girthCurves[(int)type].keys);
            keys.Sort((a, b) => a.time.CompareTo(b.time));
            float length = keys[keys.Count - 1].time - keys[0].time;
            for (float p = keys[0].time; p < (length+keys[0].time) * 1.1f; p += length / (float)Samples) {
                Vector3 center = d.dickTransform.position + d.GetXYZOffsetWorld(p, type);
                float girth = d.GetGirthWorld(p, type);
                Handles.DrawWireDisc(center, d.dickTransform.TransformDirection(forward), girth * 0.5f);
            }
        }
        int GetBoneID(SkinnedMeshRenderer m, Transform bone) {
            for (int i = 0; i < m.bones.Length; i++) {
                if (m.bones[i] == bone) {
                    return i;
                }
            }
            return -1;
        }
        public static HashSet<Transform> GetHashSetChildrenBone( Transform bone ) {
            HashSet<Transform> set = new HashSet<Transform>();
            set.Add(bone);
            for(int i=0;i<bone.childCount;i++ ) {
                GetHashSetChildrenBone(bone.GetChild(i), set);
            }
            return set;
        }
        public static void GetHashSetChildrenBone( Transform bone, HashSet<Transform> set ) {
            set.Add(bone);
            for(int i=0;i<bone.childCount;i++ ) {
                GetHashSetChildrenBone(bone.GetChild(i), set);
            }
        }
        public static Transform MeshBoneIndexToTransform(SkinnedMeshRenderer sm, int i) {
            return sm.bones[i];
        }
        public static int MeshBoneTransformToIndex(SkinnedMeshRenderer sm, Transform t) {
            for ( int i=0;i<sm.bones.Length;i++ ) {
                if ( sm.bones[i] == t) {
                    return i;
                }
            }
            return -1;
        }
        void GenerateCurves(int crossSections, Naelstrof.Dick.BlendshapeType t = (Naelstrof.Dick.BlendshapeType)(-1)) {
            if (girthCurves.arraySize != Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length) {
                girthCurves.ClearArray();
                for (int i = 0; i < Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length; i++) {
                    girthCurves.InsertArrayElementAtIndex(i);
                }
            }
            if (xOffsetCurves.arraySize != Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length) {
                xOffsetCurves.ClearArray();
                for (int i = 0; i < Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length; i++) {
                    xOffsetCurves.InsertArrayElementAtIndex(i);
                }
            }
            if (yOffsetCurves.arraySize != Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length) {
                yOffsetCurves.ClearArray();
                for (int i = 0; i < Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length; i++) {
                    yOffsetCurves.InsertArrayElementAtIndex(i);
                }
            }
            Transform dickBone = (Transform)dickTransform.objectReferenceValue;
            HashSet<Transform> dickBoneSet = GetHashSetChildrenBone(dickBone);
            foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                if (t != (Naelstrof.Dick.BlendshapeType)(-1) && t != type) {
                    continue;
                }
                //List<int> blendshapeIDs = ((Naelstrof.Dick)serializedObject.targetObject).GetBlendshapeIDsForType(type);
                List<Vector3> dickVerts = new List<Vector3>();
                for (int i = 0; i < bakeMeshes.arraySize; i++) {
                    SkinnedMeshRenderer sm = ((SkinnedMeshRenderer)(bakeMeshes.GetArrayElementAtIndex(i).objectReferenceValue));
                    int dickBoneID = MeshBoneTransformToIndex(sm, (Transform)dickTransform.objectReferenceValue);
                    Mesh m = sm.sharedMesh;

                    List<Vector3> verts = new List<Vector3>();
                    m.GetVertices(verts);
                    if (type != Naelstrof.Dick.BlendshapeType.None && blendshapeIDs.GetArrayElementAtIndex(i * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length + (int)type).intValue != 0) {
                        DickEditor.ApplyBlendshape(verts, m, blendshapeIDs.GetArrayElementAtIndex(i * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length + (int)type).intValue - 1);
                    }
                    var weights = m.GetAllBoneWeights();
                    var bonesPerVertex = m.GetBonesPerVertex();
                    int vt = 0;
                    int wt = 0;
                    for (int o = 0; o < bonesPerVertex.Length; o++) {
                        for (int p = 0; p < bonesPerVertex[o]; p++) {
                            BoneWeight1 weight = weights[wt];
                            Transform db = MeshBoneIndexToTransform(sm, weights[wt].boneIndex);
                            if (dickBoneSet.Contains(db) && weights[wt].weight > 0f) {
                                dickVerts.Add(m.bindposes[dickBoneID].MultiplyPoint(verts[vt]));
                            }
                            wt++;
                        }
                        vt++;
                    }
                }
                Vector3 forwardAxis = dickForwardAxis.vector3Value;
                Vector3 upAxis = dickUpAxis.vector3Value;
                Vector3 rightAxis = Vector3.Cross(forwardAxis, upAxis);
                if (dickVerts.Count <= 0) {
                    throw new UnityException("There was no dick verts found weighted to the target transform or its children! Make sure they have a weight assigned in the mesh.");
                }
                dickVerts.Sort((a, b) => Vector3.Dot(a, forwardAxis).CompareTo(Vector3.Dot(b, forwardAxis)));
                foreach (Vector3 vert in dickVerts) {
                    Debug.DrawLine(dickBone.TransformPoint(vert), dickBone.TransformPoint(vert) + new Vector3(0, 0.01f, 0), Color.red, 1f);
                }
                float start = Vector3.Dot(dickVerts[0], forwardAxis);
                float length = Vector3.Dot(dickVerts[dickVerts.Count - 1], forwardAxis) - start;

                AnimationCurve girthCurve = new AnimationCurve();
                AnimationCurve xOffsetCurve = new AnimationCurve();
                AnimationCurve yOffsetCurve = new AnimationCurve();
                girthCurve.AddKey(new Keyframe(length + start, 0));
                int vertexIndex = 0;
                for (float plane = start + (length / (float)crossSections); plane < length + start + (length / (float)crossSections); plane += (length / (float)crossSections)) {
                    //Debug.DrawLine(dickBone.position + dickBone.TransformDirection(forwardAxis) * plane, dickBone.position + dickBone.TransformDirection(forwardAxis) * plane + new Vector3(0, 0.1f, 0), Color.blue, 1f);
                    List<Vector3> crossSection = new List<Vector3>();
                    while (vertexIndex < dickVerts.Count && Vector3.Dot(dickVerts[vertexIndex], forwardAxis) < plane) {
                        crossSection.Add(dickVerts[vertexIndex++]);
                    }
                    //float halfMeasure = (length / (float)crossSections) * 0.5f;
                    float time = plane - (length / (float)crossSections);
                    // Make sure we have a few verts in our cross section.
                    int tempVertexIndex = vertexIndex;
                    int otherTempVertexIndex = vertexIndex - 1;
                    // Search both ways to make sure we get at least a few
                    while (tempVertexIndex < dickVerts.Count && otherTempVertexIndex > 0 && otherTempVertexIndex < dickVerts.Count && crossSection.Count <= 64) {
                        crossSection.Add(dickVerts[tempVertexIndex++]);
                        crossSection.Add(dickVerts[otherTempVertexIndex--]);
                    }

                    crossSection.Sort((a, b) => Vector3.Dot(a, rightAxis).CompareTo(Vector3.Dot(b, rightAxis)));
                    float crossWidth = Vector3.Dot(crossSection[crossSection.Count - 1], rightAxis) - Vector3.Dot(crossSection[0], rightAxis);
                    float crossRightCenter = Vector3.Dot(crossSection[0], rightAxis) + crossWidth / 2f;
                    crossSection.Sort((a, b) => Vector3.Dot(a, upAxis).CompareTo(Vector3.Dot(b, upAxis)));
                    float crossHeight = Vector3.Dot(crossSection[crossSection.Count - 1], upAxis) - Vector3.Dot(crossSection[0], upAxis);
                    float crossHeightCenter = Vector3.Dot(crossSection[0], upAxis) + crossHeight / 2f;
                    girthCurve.AddKey(new Keyframe(time, (crossWidth + crossHeight) * 0.5f));
                    xOffsetCurve.AddKey(new Keyframe(time, crossRightCenter));
                    yOffsetCurve.AddKey(new Keyframe(time, crossHeightCenter));
                    //if (type == Naelstrof.Dick.BlendshapeType.None) {
                    //DrawCrossSection(forwardAxis, rightAxis, upAxis, new Vector3(crossRightCenter, crossHeightCenter, time), (crossWidth + crossHeight) * 0.5f, Color.gray);
                    //}
                }
                SmoothCurve(girthCurve);
                SmoothCurve(xOffsetCurve);
                SmoothCurve(yOffsetCurve);

                girthCurves.GetArrayElementAtIndex((int)type).animationCurveValue = girthCurve;
                xOffsetCurves.GetArrayElementAtIndex((int)type).animationCurveValue = xOffsetCurve;
                yOffsetCurves.GetArrayElementAtIndex((int)type).animationCurveValue = yOffsetCurve;
            }
        }
        public static void GenerateCurves(Dick target, int crossSections, Naelstrof.Dick.BlendshapeType t = (Naelstrof.Dick.BlendshapeType)(-1)) {
            if (target.girthCurves.Count != Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length) {
                target.girthCurves.Clear();
                for (int i = 0; i < Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length; i++) {
                    target.girthCurves.Add(new AnimationCurve());
                }
            }
            if (target.xOffsetCurves.Count != Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length) {
                target.xOffsetCurves.Clear();
                for (int i = 0; i < Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length; i++) {
                    target.xOffsetCurves.Add(new AnimationCurve());
                }
            }
            if (target.yOffsetCurves.Count != Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length) {
                target.yOffsetCurves.Clear();
                for (int i = 0; i < Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length; i++) {
                    target.yOffsetCurves.Add(new AnimationCurve());
                }
            }
            Transform dickBone = target.dickTransform;
            HashSet<Transform> dickBoneSet = GetHashSetChildrenBone(dickBone);
            foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                if (t != (Naelstrof.Dick.BlendshapeType)(-1) && t != type) {
                    continue;
                }
                //List<int> blendshapeIDs = ((Naelstrof.Dick)serializedObject.targetObject).GetBlendshapeIDsForType(type);
                List<Vector3> dickVerts = new List<Vector3>();
                for (int i = 0; i < target.bakeMeshes.Count; i++) {
                    SkinnedMeshRenderer sm = ((SkinnedMeshRenderer)(target.bakeMeshes[i]));
                    int dickBoneID = MeshBoneTransformToIndex(sm, target.dickTransform);
                    Mesh m = sm.sharedMesh;

                    List<Vector3> verts = new List<Vector3>();
                    m.GetVertices(verts);
                    if (type != Naelstrof.Dick.BlendshapeType.None && target.blendshapeIDs[i * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length + (int)type] != 0) {
                        ApplyBlendshape(verts, m, target.blendshapeIDs[i * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length + (int)type] - 1);
                    }
                    var weights = m.GetAllBoneWeights();
                    var bonesPerVertex = m.GetBonesPerVertex();
                    int vt = 0;
                    int wt = 0;
                    for (int o=0;o<bonesPerVertex.Length;o++ ) {
                        for (int p = 0; p < bonesPerVertex[o]; p++) {
                            BoneWeight1 weight = weights[wt];
                            Transform db = MeshBoneIndexToTransform(sm, weights[wt].boneIndex);
                            if (dickBoneSet.Contains(db) && weights[wt].weight > 0f) {
                                dickVerts.Add(m.bindposes[dickBoneID].MultiplyPoint(verts[vt]));
                            }
                            wt++;
                        }
                        vt++;
                    }
                }
                Vector3 forwardAxis = target.dickForwardAxis;
                Vector3 upAxis = target.dickUpAxis;
                Vector3 rightAxis = Vector3.Cross(forwardAxis, upAxis);
                if (dickVerts.Count <= 0 ) {
                    throw new UnityException("There was no dick verts found weighted to the target transform or its children! Make sure they have a weight assigned in the mesh.");
                }
                dickVerts.Sort((a, b) => Vector3.Dot(a, forwardAxis).CompareTo(Vector3.Dot(b, forwardAxis)));
                foreach(Vector3 vert in dickVerts) {
                    Debug.DrawLine(dickBone.TransformPoint(vert), dickBone.TransformPoint(vert) + new Vector3(0, 0.01f, 0), Color.red, 1f);
                }
                float start = Vector3.Dot(dickVerts[0], forwardAxis);
                float length = Vector3.Dot(dickVerts[dickVerts.Count - 1], forwardAxis) - start;

                AnimationCurve girthCurve = new AnimationCurve();
                AnimationCurve xOffsetCurve = new AnimationCurve();
                AnimationCurve yOffsetCurve = new AnimationCurve();
                girthCurve.AddKey(new Keyframe(length+start, 0));
                int vertexIndex = 0;
                for (float plane = start+(length/(float)crossSections); plane < length+start + (length/(float)crossSections); plane += (length / (float)crossSections)) {
                    //Debug.DrawLine(dickBone.position + dickBone.TransformDirection(forwardAxis) * plane, dickBone.position + dickBone.TransformDirection(forwardAxis) * plane + new Vector3(0, 0.1f, 0), Color.blue, 1f);
                    List<Vector3> crossSection = new List<Vector3>();
                    while (vertexIndex < dickVerts.Count && Vector3.Dot(dickVerts[vertexIndex], forwardAxis) < plane) {
                        crossSection.Add(dickVerts[vertexIndex++]);
                    }
                    //float halfMeasure = (length / (float)crossSections) * 0.5f;
                    float time = plane - (length/(float)crossSections);
                    // Make sure we have a few verts in our cross section.
                    int tempVertexIndex = vertexIndex;
                    int otherTempVertexIndex = vertexIndex-1;
                    // Search both ways to make sure we get at least a few
                    while (tempVertexIndex < dickVerts.Count && otherTempVertexIndex > 0 && otherTempVertexIndex < dickVerts.Count && crossSection.Count <= 64) {
                        crossSection.Add(dickVerts[tempVertexIndex++]);
                        crossSection.Add(dickVerts[otherTempVertexIndex--]);
                    }

                    crossSection.Sort((a, b) => Vector3.Dot(a, rightAxis).CompareTo(Vector3.Dot(b, rightAxis)));
                    float crossWidth = Vector3.Dot(crossSection[crossSection.Count - 1], rightAxis) - Vector3.Dot(crossSection[0], rightAxis);
                    float crossRightCenter = Vector3.Dot(crossSection[0], rightAxis) + crossWidth / 2f;
                    crossSection.Sort((a, b) => Vector3.Dot(a, upAxis).CompareTo(Vector3.Dot(b, upAxis)));
                    float crossHeight = Vector3.Dot(crossSection[crossSection.Count - 1], upAxis) - Vector3.Dot(crossSection[0], upAxis);
                    float crossHeightCenter = Vector3.Dot(crossSection[0], upAxis) + crossHeight / 2f;
                    girthCurve.AddKey(new Keyframe(time, (crossWidth + crossHeight) * 0.5f));
                    xOffsetCurve.AddKey(new Keyframe(time, crossRightCenter));
                    yOffsetCurve.AddKey(new Keyframe(time, crossHeightCenter));
                    //if (type == Naelstrof.Dick.BlendshapeType.None) {
                    //DrawCrossSection(forwardAxis, rightAxis, upAxis, new Vector3(crossRightCenter, crossHeightCenter, time), (crossWidth + crossHeight) * 0.5f, Color.gray);
                    //}
                }
                SmoothCurve(girthCurve);
                SmoothCurve(xOffsetCurve);
                SmoothCurve(yOffsetCurve);
                target.girthCurves[(int)type] = girthCurve;
                target.xOffsetCurves[(int)type] = xOffsetCurve;
                target.yOffsetCurves[(int)type] = yOffsetCurve;
            }
        }

        List<Mesh> GetMeshes() {
            List<Mesh> meshes = new List<Mesh>();
            for (int i = 0; i < bakeMeshes.arraySize; i++) {
                if (bakeMeshes.GetArrayElementAtIndex(i).objectReferenceValue != null) {
                    meshes.Add(((SkinnedMeshRenderer)(bakeMeshes.GetArrayElementAtIndex(i).objectReferenceValue)).sharedMesh);
                } else {
                    meshes.Add(null);
                }
            }
            return meshes;
        }
        List<string> GetOptions(Mesh m) {
            List<string> options = new List<string>();
            options.Add("None");
            for (int o = 0; o < m.blendShapeCount; o++) {
                options.Add(m.GetBlendShapeName(o));
            }
            return options;
        }

        void BlendshapeField(List<Mesh> meshes, Naelstrof.Dick.BlendshapeType type) {
            string typeString = Naelstrof.Dick.BlendshapeTypeToString(type);
            string blendshapeTarget = "???";
            string toolTipString = "???";
            switch (type) {
                case Naelstrof.Dick.BlendshapeType.Cum:
                    toolTipString = "Blendshape that gets triggered when the dick is pumping cum, for most dicks this will be a really subtle bulge of the urethra.";
                    blendshapeTarget = "DickCum";
                    break;
                case Naelstrof.Dick.BlendshapeType.Pull:
                    toolTipString = "Blendshape that gets triggered when the dick is getting pulled out, usually this stretches the dick out a bit.";
                    blendshapeTarget = "DickPull";
                    break;
                case Naelstrof.Dick.BlendshapeType.Squish:
                    blendshapeTarget = "DickSquish";
                    toolTipString = "Blendshape that gets triggered when the dick is getting pushed in, usually this squishes the dick down a bit.";
                    break;
                default:
                    break;
            }
            EditorGUILayout.LabelField(new GUIContent(typeString + " Blendshapes", toolTipString));
            EditorGUI.indentLevel++;
            //List<int> blendshapeIDs = ((Naelstrof.Dick)serializedObject.targetObject).GetBlendshapeIDsForType(type);
            //((Naelstrof.Dick)serializedObject.targetObject).GetBlendshapeIDsForType(type);
            //List<int> blendshapeIDs = (List<int>)(blendshapeID.GetArrayElementAtIndex((int)type).array);
            //List<int> blendshapeIDs = ((Naelstrof.Dick)serializedObject.targetObject).GetBlendshapeIDsForType(type);
            if (blendshapeIDs.arraySize != meshes.Count * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length) {
                blendshapeIDs.ClearArray();
                for (int i = 0; i < meshes.Count * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length; i++) {
                    blendshapeIDs.InsertArrayElementAtIndex(i);
                }
            }
            for (int i = 0; i < meshes.Count; i++) {
                Mesh m = meshes[i];
                if (m != null) {
                    int id = blendshapeIDs.GetArrayElementAtIndex(i * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length + (int)type).intValue;
                    List<string> options = GetOptions(m);
                    if (id==0) {
                        if (options.Contains(blendshapeTarget)) {
                            id = options.IndexOf(blendshapeTarget);
                        }
                    }
                    blendshapeIDs.GetArrayElementAtIndex(i * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length + (int)type).intValue = EditorGUILayout.Popup(new GUIContent((m.name + "'s " + typeString), toolTipString), id, GetOptions(m).ToArray());
                }
            }
            EditorGUI.indentLevel--;
        }

        public override void OnInspectorGUI() {
            Naelstrof.Dick d = ((Naelstrof.Dick)serializedObject.targetObject);
            serializedObject.Update();
            Undo.RecordObject(serializedObject.targetObject, "Changes to " + serializedObject.targetObject.name);
            //EditorGUILayout.PropertyField(dickForwardAxis);
            //EditorGUILayout.PropertyField(dickUpAxis);
            //d.dickRightAxis = Vector3.Cross(d.dickForwardAxis, d.dickUpAxis);
            Vector3.OrthoNormalize(ref d.dickForwardAxis, ref d.dickUpAxis, ref d.dickRightAxis);
            if (dickTransform.objectReferenceValue == null || bakeMeshes.arraySize == 0) {
                EditorGUILayout.HelpBox("Set both the dick armature bone, as well as the bake meshes. The dick can be composed of several meshes.", MessageType.Warning);
            }
            if ( deformationTargets.arraySize <= 0 ) {
                EditorGUILayout.HelpBox("The dick is missing deformation targets (Preview Deformations Tab), these are necessary if you want the dick to dynamically compress/stretch.", MessageType.Warning);
            }
            //EditorGUILayout.PropertyField(dickTransform, new GUIContent("Dick Transform", "This is the transform that can point the dick toward holes. It's really important for both baking and runtime for this to be set correctly."));

            if (dickTransform.objectReferenceValue != null && bakeMeshes.arraySize != 0) {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                tab = GUILayout.Toolbar(tab, new string[] { "Bake", "Preview Bake", "Preview Deformations" });
                switch (tab) {
                    case 0:
                        List<Mesh> meshes = GetMeshes();
                        //EditorGUILayout.HelpBox("Use the below drop-downs to select which blendshapes are used for squishing, confining, pulling, and cumming. Leave them at \"None\" if they don't have a blendshape for that.", MessageType.Info);
                        BlendshapeField(meshes, Naelstrof.Dick.BlendshapeType.Squish);
                        BlendshapeField(meshes, Naelstrof.Dick.BlendshapeType.Pull);
                        BlendshapeField(meshes, Naelstrof.Dick.BlendshapeType.Cum);
                        crossSectionCount = EditorGUILayout.IntSlider(new GUIContent("Cross Sections", "The number of cross sections to analyze from the dick verts, high values might cause \"pinching\". Low values might be inaccurate."), crossSectionCount, 0, 32);
                        generateAll = EditorGUILayout.Toggle(new GUIContent("Generate All Curves", "Check this to override all curves, if you've made adjustments you'll lose them!"), generateAll);
                        if (generateAll) {
                            if (GUILayout.Button("Generate All Dick Curves")) {
                                //try {
                                    GenerateCurves(crossSectionCount);
                                    bakeStatus = 1;
                                //} catch (Exception e) {
                                    //bakeStatus = -1;
                                    //throw e;
                                //}
                            }
                        } else {
                            List<string> bakeOptions = new List<string>();
                            foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                                bakeOptions.Add(Naelstrof.Dick.BlendshapeTypeToString(type));
                            }
                            generateType = (Naelstrof.Dick.BlendshapeType)EditorGUILayout.Popup("Generate Curve Target", (int)generateType, bakeOptions.ToArray());
                            if (GUILayout.Button("Generate " + bakeOptions[(int)generateType] + " Curves")) {
                                try {
                                    GenerateCurves(crossSectionCount, generateType);
                                    bakeStatus = 1;
                                } catch (Exception e) {
                                    bakeStatus = -1;
                                    throw e;
                                }
                            }
                        }
                        switch (bakeStatus) {
                            case -1:
                                EditorGUILayout.HelpBox("Failed to bake, check errors in console!", MessageType.Error);
                                break;
                            case 0:
                                break;
                            case 1:
                                EditorGUILayout.HelpBox("Bake success! Head to the Preview Bake tab to adjust them.", MessageType.Info);
                                break;
                        }

                        foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                            SetBlends(d, type, 0f);
                        }
                        break;
                    case 1:
                        bakeStatus = 0;
                        if (d.girthCurves.Count <= 0) {
                            EditorGUILayout.HelpBox("There's no baked data yet. Click on the bake tab above to get started.", MessageType.Warning);
                            break;
                        }
                        EditorGUILayout.HelpBox("Here you can preview the bakes, and make adjustments to them.", MessageType.Info);
                        List<string> options = new List<string>();
                        foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                            options.Add(Naelstrof.Dick.BlendshapeTypeToString(type));
                        }
                        previewID = EditorGUILayout.Popup("Preview Type", previewID, options.ToArray());
                        string toolTip = "Use these curves to adjust the dick shape. Use the preview in the 3D view to see the changes.";
                        girthCurves.GetArrayElementAtIndex((int)previewID).animationCurveValue = EditorGUILayout.CurveField(new GUIContent(Naelstrof.Dick.BlendshapeTypeToString((Naelstrof.Dick.BlendshapeType)previewID) + "'s Girth", toolTip), girthCurves.GetArrayElementAtIndex((int)previewID).animationCurveValue);
                        xOffsetCurves.GetArrayElementAtIndex((int)previewID).animationCurveValue = EditorGUILayout.CurveField(new GUIContent(Naelstrof.Dick.BlendshapeTypeToString((Naelstrof.Dick.BlendshapeType)previewID) + "'s X Position", toolTip), xOffsetCurves.GetArrayElementAtIndex((int)previewID).animationCurveValue);
                        yOffsetCurves.GetArrayElementAtIndex((int)previewID).animationCurveValue = EditorGUILayout.CurveField(new GUIContent(Naelstrof.Dick.BlendshapeTypeToString((Naelstrof.Dick.BlendshapeType)previewID) + "'s Y Position", toolTip), yOffsetCurves.GetArrayElementAtIndex((int)previewID).animationCurveValue);
                        //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        break;
                    case 2:
                        bakeStatus = 0;
                        if (deformationTargets.arraySize == 0) {
                            EditorGUILayout.HelpBox("Please set a deformation target renderer below.", MessageType.Warning);
                            break;
                        }

                        EditorGUILayout.HelpBox("Here you can set which render targets get sent deformation data, and preview how they'd look.", MessageType.Info);
                        EditorGUILayout.PropertyField(deformationTargets, new GUIContent("Deformation Targets", "These renderer's materials will be sent dick deformation data. Make sure they have a compatible dick deformation shader on them if you want them to deform"), true);

                        HashSet<string> nameTest = new HashSet<string>(new string[]{ "DickSquish", "DickCum", "DickPull"});
                        for (int i = 0; i < deformationTargets.arraySize; i++) {
                            SkinnedMeshRenderer r = (SkinnedMeshRenderer)deformationTargets.GetArrayElementAtIndex(i).objectReferenceValue;
                            if (r == null ) {
                                continue;
                            }
                            List<Material> materials = new List<Material>();
                            bool foundBlendShapeNames = false;
                            for ( int o=0;o<r.sharedMesh.blendShapeCount;o++ ) {
                                if (nameTest.Contains(r.sharedMesh.GetBlendShapeName(o))) {
                                    foundBlendShapeNames = true;
                                    break;
                                }
                            }
                            r.GetSharedMaterials(materials);
                            bool foundDickShader = false;
                            foreach (Material m in materials) {
                                if (m.shader == Shader.Find("Naelstrof/DickDeformation")) {
                                    foundDickShader = true;
                                    break;
                                }
                            }
                            if (!foundDickShader) {
                                EditorGUILayout.HelpBox("Deformation Target: " + r.name + " doesn't have the Naelstrof/DickDeformation shader applied. It won't recieve deformation information.", MessageType.Warning);
                            }
                            if (!foundBlendShapeNames) {
                                EditorGUILayout.HelpBox("Deformation Target: " + r.name + " doesn't have the correct blendshape names. It requires \"DickSquish\", \"DickCum\", and \"DickPull\" blendshapes.", MessageType.Warning);
                            }
                        }

                        if ( d.girthCurves.Count <= 0) {
                            EditorGUILayout.HelpBox("There's no baked data yet. Click on the bake tab above to get started.", MessageType.Warning);
                            break;
                        }
                        List<string> deformOptions = new List<string>();
                        foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                            deformOptions.Add("Preview " + Naelstrof.Dick.BlendshapeTypeToString(type) + " Deform");
                        }
                        //EditorGUILayout.PropertyField(blendshapeSoftness, new GUIContent("Blendshape Softness", "This variable adjusts how large of an area around the hole is affected by squish/pull/cum effects. Adjust it until only an area around the dick during deformation is affected to taste."), true);
                        previewDeformation = EditorGUILayout.Popup("Preview Type", previewDeformation, deformOptions.ToArray());
                        if (previewDeformation != (int)Naelstrof.Dick.BlendshapeType.Cum) {
                            previewPlaneSlider = EditorGUILayout.Slider(new GUIContent("Preview Insertion", "Moves the point from which a hole might be to test the deformations."), previewPlaneSlider, d.GetMinPenetrationDepth(), d.GetMaxLength());
                        } else {
                            previewPlaneSlider = EditorGUILayout.Slider(new GUIContent("Preview Cum", "Shows what the cumming animation might look like, slide it back to front."), previewPlaneSlider, d.GetMinPenetrationDepth(), d.GetMaxLength());
                        }
                        break;
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            } else {
                tab = 0;
            }
            //if (d.girthCurves.Count != 0) {
                //EditorGUILayout.HelpBox("These options are meant to be changed during run-time. They won't have an effect outside of play-mode.", MessageType.Info);
                //DrawDefaultInspector();
                DrawPropertiesExcluding(serializedObject, "girthCurves", "xOffsetCurves", "yOffsetCurves");
                //EditorGUILayout.PropertyField(holeTarget, new GUIContent("Hole Target", "This is the hole that the dick will attempt to point at and manipulate."));
                //EditorGUILayout.PropertyField(cumProgress);
                //EditorGUILayout.PropertyField(cumActive);
                //EditorGUILayout.PropertyField(OnPenetrate);
                //EditorGUILayout.PropertyField(OnDepenetrate);
                //EditorGUILayout.PropertyField(PenetrateContinuous);
                //EditorGUILayout.PropertyField(stream);
                //EditorGUILayout.PropertyField(strandMaterial);
                //EditorGUILayout.PropertyField(body);
                //EditorGUILayout.PropertyField(hitBoxCollider);
                //EditorGUILayout.PropertyField(pumpingSounds);
                //EditorGUILayout.PropertyField(plappingSounds);
                //EditorGUILayout.PropertyField(OnMove);
                //EditorGUILayout.PropertyField(balls);
                //EditorGUILayout.PropertyField(kobold);
            //}
            serializedObject.ApplyModifiedProperties();
            if (Application.isPlaying) {
                return;
            }
            if (tab == 0) {
                List<float> weights = new List<float>();
                foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                    weights.Add(0f);
                }
                foreach (SkinnedMeshRenderer r in d.deformationTargets) {
                    SetMeshStuff(d, r, weights);
                }
            }
            if (tab == 1 && d.girthCurves.Count != 0) {
                List<float> weights = new List<float>();
                foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                    weights.Add(0f);
                }
                foreach (SkinnedMeshRenderer r in d.deformationTargets) {
                    SetMeshStuff(d, r, weights);
                }
                foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                    if (type == (Naelstrof.Dick.BlendshapeType)previewID) {
                        SetBlends(d, type, 100f);
                    } else {
                        SetBlends(d, type, 0f);
                    }
                }
            }
            if (tab == 2) {
                foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                    SetBlends(d, type, 0f);
                }
                Vector3 forward = d.dickTransform.TransformDirection(d.dickForwardAxis);
                List<float> weights = new List<float>();
                foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                    if ((int)type == previewDeformation) {
                        weights.Add(1);
                    } else {
                        weights.Add(0);
                    }
                }
                foreach (SkinnedMeshRenderer r in d.deformationTargets) {
                    if (r == null) {
                        continue;
                    }
                    SetMeshStuff(d, r, weights);
                }
            }
        }
        public void SetMeshStuff(Naelstrof.Dick d, SkinnedMeshRenderer r, List<float> weights) {
            if ( r == null) {
                return;
            }
            foreach (Material m in r.sharedMaterials) {
                m.SetFloat("_SquishAmount", weights[(int)Naelstrof.Dick.BlendshapeType.Squish]);
                m.SetFloat("_PullAmount", weights[(int)Naelstrof.Dick.BlendshapeType.Pull]);
                m.SetFloat("_CumAmount", weights[(int)Naelstrof.Dick.BlendshapeType.Cum]);
                m.SetFloat("_HoleProgress", previewPlaneSlider * d.dickTransform.lossyScale.x);
                m.SetFloat("_CumProgress", previewPlaneSlider * d.dickTransform.lossyScale.x);
                m.SetFloat("_ModelScale", blendshapeSoftness.floatValue * d.dickTransform.lossyScale.x);//d.GetWorldLength(weights)*d.dickTransform.lossyScale.x*3f);
                for(int i=0;i<r.sharedMesh.bindposes.Length;i++ ) {
                    if ( r.bones[i] == d.dickTransform ) {
                        m.SetFloat("_BlendshapeMultiplier", d.dickTransform.lossyScale.x*r.sharedMesh.bindposes[i].lossyScale.x);
                        break;
                    }
                }
                m.SetVector("_DickOrigin", r.rootBone.worldToLocalMatrix.MultiplyPoint(d.dickTransform.position) * r.rootBone.lossyScale.x);
                m.SetVector("_DickForward", Vector3.Normalize(r.rootBone.worldToLocalMatrix.MultiplyVector(d.dickTransform.TransformDirection(d.dickForwardAxis))));
                m.SetVector("_DickRight", Vector3.Normalize(r.rootBone.worldToLocalMatrix.MultiplyVector(d.dickTransform.TransformDirection(d.dickRightAxis))));
                m.SetVector("_DickUp", Vector3.Normalize(r.rootBone.worldToLocalMatrix.MultiplyVector(d.dickTransform.TransformDirection(d.dickUpAxis))));
                //m.SetFloat("_DickLength", d.GetLocalLength(d.weights));
            }
        }
        public void SetBlends(Naelstrof.Dick d, Naelstrof.Dick.BlendshapeType type, float amount) {
            if (type == Naelstrof.Dick.BlendshapeType.None) {
                return;
            }
            for (int i = 0; i < bakeMeshes.arraySize; i++) {
                int blendID = blendshapeIDs.GetArrayElementAtIndex(i * Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType)).Length + (int)type).intValue - 1;
                if (blendID == -1) {
                    continue;
                }
                if (bakeMeshes.GetArrayElementAtIndex(i).objectReferenceValue != null) {
                    ((SkinnedMeshRenderer)(bakeMeshes.GetArrayElementAtIndex(i).objectReferenceValue)).SetBlendShapeWeight(blendID, amount);
                }
            }
        }
        public void OnSceneGUI() {
            if (Application.isPlaying) {
                return;
            }
            Naelstrof.Dick d = ((Naelstrof.Dick)serializedObject.targetObject);
            if (tab == 1 && d.girthCurves.Count != 0) {
                List<float> weights = new List<float>();
                foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                    weights.Add(0f);
                }
                DrawApprox((Naelstrof.Dick.BlendshapeType)previewID, 32);
            }
            if (tab == 2) {
                Vector3 forward = d.dickTransform.TransformDirection(d.dickForwardAxis);
                List<float> weights = new List<float>();
                foreach (Naelstrof.Dick.BlendshapeType type in Enum.GetValues(typeof(Naelstrof.Dick.BlendshapeType))) {
                    if ((int)type == previewDeformation) {
                        weights.Add(1);
                    } else {
                        weights.Add(0);
                    }
                }
                if(d.girthCurves.Count <= 0) {
                    return;
                }
                float girth = d.GetGirthWorld(previewPlaneSlider, weights);
                Vector3 p = d.GetXYZOffsetWorld(previewPlaneSlider, weights);
                Handles.DrawWireDisc(d.dickTransform.position + p, forward, girth * 0.5f);
            }
        }
    }
}
#endif
