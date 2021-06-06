using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Naelstrof;
using System;

#if UNITY_EDITOR
using UnityEditor;
public static class AnimationClipCurveDataExtension {
    public static EditorCurveBinding GetBinding(this AnimationClipCurveData obj) {
        EditorCurveBinding binding = new EditorCurveBinding();
        binding.propertyName = obj.propertyName;
        binding.path = obj.path;
        binding.type = obj.type;
        return binding;
    }
}
[CustomEditor(typeof(DickAnimationBaker))]
public class DickAnimationBakerEditor : Editor {
    SerializedProperty OriginalAnimations;
    SerializedProperty SaveOutFolder;
    SerializedProperty SampleRate;
    void OnEnable() {
        OriginalAnimations = serializedObject.FindProperty("OriginalAnimations");
        SaveOutFolder = serializedObject.FindProperty("SaveOutFolder");
        SampleRate = serializedObject.FindProperty("SampleRate");
    }
    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(OriginalAnimations, true);
        EditorGUILayout.PropertyField(SaveOutFolder);
        EditorGUILayout.PropertyField(SampleRate);
        EditorGUILayout.HelpBox("Pressing Bake Animations will enter playmode to bake the animations. Make sure to remove this script when you're done as it will always bake whenever you enter playmode.", MessageType.Info);
        if (GUILayout.Button("Bake Animations")) {
            ((DickAnimationBaker)serializedObject.targetObject).Bake();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
public class DickAnimationBaker : MonoBehaviour {
    [Tooltip("The list of animations to convert.")]
    public List<AnimationClip> OriginalAnimations = new List<AnimationClip>();
    [Tooltip("If the baked animation turns out inaccurate (laggy), you can increase the sample rate here.")]
    public float SampleRate = 15f;
    [Tooltip("The folder to output the animations, the resulting animations have the same name ending with `Baked`.")]
    public string SaveOutFolder = "Assets/";

    private List<Naelstrof.Dick> dicks;
    private List<Naelstrof.Penetratable> penetratables;

#if UNITY_EDITOR
    public static AnimationClipCurveData NewAnimationClipCurveData(Type type, string path, string property) {
        AnimationClipCurveData blah = new AnimationClipCurveData();
        blah.type = type;
        blah.path = path;
        blah.propertyName = property;
        blah.curve = new AnimationCurve();
        return blah;
    }
    public string GetGameObjectPath(GameObject obj) {
        string path = "/" + obj.name;
        while (obj.transform.parent != transform) {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path.Remove(0,1);
    }
    public void Start() {
        StartCoroutine(BakePlaying());
    }
    public void Bake() {
        GetComponent<Animator>().runtimeAnimatorController = null;
        EditorApplication.isPlaying = true;
    }
    public AnimationClip CopyAnimation(AnimationClip clip) {
        List<string> pathBlacklist = new List<string>();
        List<string> propertyBlacklist = new List<string>();
        AnimationClip newClip = new AnimationClip();
        newClip.frameRate = clip.frameRate;
        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip)) {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
            foreach (string s in propertyBlacklist) {
                if (binding.propertyName.StartsWith(s)) {
                    continue;
                }
            }
            foreach (string s in pathBlacklist) {
                if (binding.path.StartsWith(s)) {
                    continue;
                }
            }
            // Copy curves
            newClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
        }
        return newClip;
    }
    public IEnumerator BakePlaying() {
        // Wait one frame for stuff to Start()
        yield return new WaitForEndOfFrame();

        // Stuff to record ----------------------------------------------------------------------
        List<AnimationClipCurveData> stuffToRecord = new List<AnimationClipCurveData>();
        dicks = new List<Naelstrof.Dick>(GetComponentsInChildren<Naelstrof.Dick>());
        penetratables = new List<Naelstrof.Penetratable>(GetComponentsInChildren<Naelstrof.Penetratable>());
        foreach(Dick d in dicks) {
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(d.dickTransform.gameObject), "m_LocalRotation.x"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(d.dickTransform.gameObject), "m_LocalRotation.y"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(d.dickTransform.gameObject), "m_LocalRotation.z"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(d.dickTransform.gameObject), "m_LocalRotation.w"));
            foreach(SkinnedMeshRenderer r in d.deformationTargets) {
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._DickOrigin.x"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._DickOrigin.y"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._DickOrigin.z"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._DickForward.x"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._DickForward.y"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._DickForward.z"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._ModelScale"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._HoleProgress"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._PullAmount"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._SquishAmount"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._BlendshapeMultiplier"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._CumProgress"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._CumAmount"));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "material._ConfineAmount"));
            }
        }
        foreach(Penetratable p in penetratables) {
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(p.holeTransform.gameObject), "m_LocalPosition.x"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(p.holeTransform.gameObject), "m_LocalPosition.y"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(p.holeTransform.gameObject), "m_LocalPosition.z"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(p.holeTransform.gameObject), "m_LocalRotation.x"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(p.holeTransform.gameObject), "m_LocalRotation.y"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(p.holeTransform.gameObject), "m_LocalRotation.z"));
            stuffToRecord.Add(NewAnimationClipCurveData(typeof(Transform), GetGameObjectPath(p.holeTransform.gameObject), "m_LocalRotation.w"));
            foreach (SkinnedMeshRenderer r in p.holeMeshes) {
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "blendShape." + p.expandBlendshapeName));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "blendShape." + p.pullBlendshapeName));
                stuffToRecord.Add(NewAnimationClipCurveData(typeof(SkinnedMeshRenderer), GetGameObjectPath(r.gameObject), "blendShape." + p.pushBlendshapeName));
            }
        }
        // End Stuff to record -------------------------------------------------------------------
        foreach (AnimationClip clip in OriginalAnimations) {
            AnimationClip newClip = CopyAnimation(clip);
            Time.captureDeltaTime = 1f / SampleRate;
            for (float t = 0; t <= clip.length; t += 1f / SampleRate) {
                // Play the animation back.
                clip.SampleAnimation(gameObject, t);
                // Wait a frame for scripts to upate.
                yield return new WaitForEndOfFrame();
                foreach(AnimationClipCurveData data in stuffToRecord) {
                    float sample;
                    AnimationUtility.GetFloatValue(gameObject, data.GetBinding(), out sample);
                    data.curve.AddKey(t, sample);
                }
            }
            foreach( AnimationClipCurveData data in stuffToRecord) {
                for (int i = 0; i < data.curve.length; i++) {
                    AnimationUtility.SetKeyLeftTangentMode(data.curve, i, AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(data.curve, i, AnimationUtility.TangentMode.ClampedAuto);
                }
                newClip.SetCurve(data.path, data.type, data.propertyName, data.curve);
            }
            newClip.EnsureQuaternionContinuity();
            // Save the clip out
            AssetDatabase.CreateAsset(newClip, SaveOutFolder + "/" + clip.name + "Baked.anim");
            AssetDatabase.SaveAssets();
            Debug.Log("Saved to " + SaveOutFolder + "/" + clip.name + "Baked.anim :thumbsup:");
        }
        Time.captureDeltaTime = 0f;
        EditorApplication.isPlaying = false;
    }
#endif
}
