using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GenericInflatable : MonoBehaviour {
    private WaitForEndOfFrame waitForEndOfFrame;
    [System.Serializable]
    public class InflatableBlendshape {
        public InflatableBlendshape() { }
        public InflatableBlendshape(AnimationCurve curve, string blendshapeName) {
            this.curve = curve;
            this.blendshapeName = blendshapeName;
        }
        public AnimationCurve curve;
        public string blendshapeName;
        [HideInInspector]
        public Dictionary<SkinnedMeshRenderer, int> blendshapeIDs = new Dictionary<SkinnedMeshRenderer, int>();
    }
    [System.Serializable]
    public class InflatableTransform {
        public InflatableTransform() { }
        public InflatableTransform(Transform targetTransform, AnimationCurve scaleCurve, AnimationCurve rotateCurve, AnimationCurve translateOffsetCurve, Vector3 rotateAxis, Vector3 translationOffset) {
            this.targetTransform = targetTransform;
            this.scaleCurve = scaleCurve;
            this.rotateCurve = rotateCurve;
            this.translateOffsetCurve = translateOffsetCurve;
            this.rotateAxis = rotateAxis;
            this.translationOffset = translationOffset;
        }
        public AnimationCurve scaleCurve;
        public AnimationCurve rotateCurve;
        public AnimationCurve translateOffsetCurve;
        public Vector3 rotateAxis;
        public Vector3 translationOffset;
        public Transform targetTransform;
        [HideInInspector]
        public Vector3 initialScale;
        [HideInInspector]
        public Quaternion initialRotation;
        [HideInInspector]
        public Vector3 initialPosition;
    }
    [System.Serializable]
    public class InflatableSoftbody {
        public JigglePhysics.JiggleSkin targetPhysics;
        public int zoneIndex;
        public AnimationCurve blend;
        public AnimationCurve radiusCurve;
        [HideInInspector]
        public float radiusDefault;
    }
    [System.Serializable]
    public class InflatableJiggleBone {
        public JigglePhysics.JiggleRigBuilder targetRig;
        public int zoneIndex;
        public AnimationCurve blend;
    }
    [System.Serializable]
    public class InflationChangeEvent : SerializableEvent<float> { };
    [System.Serializable]
    public class InflatableChangeEventCurve {
        public InflatableChangeEventCurve() { }
        public InflatableChangeEventCurve(AnimationCurve curve, InflationChangeEvent e) {
            this.curve = curve;
            this.e = e;
        }
        public AnimationCurve curve;
        public InflationChangeEvent e;
    }

    [System.Serializable]
    public class InflatableMaterialFloatCurve {
        public AnimationCurve curve;
        public string floatPropertyName;
        public List<Renderer> targetRenderers = new List<Renderer>();
        [HideInInspector]
        public List<Material> cachedMaterials = new List<Material>();
    }

    public List<SkinnedMeshRenderer> targetRenderers = new List<SkinnedMeshRenderer>();

    public List<InflatableBlendshape> shapeCurves = new List<InflatableBlendshape>();
    public List<InflatableTransform> transformCurves = new List<InflatableTransform>();
    public List<InflatableSoftbody> softbodyCurves = new List<InflatableSoftbody>();
    public List<InflatableJiggleBone> jiggleBoneCurves = new List<InflatableJiggleBone>();
    public List<InflatableChangeEventCurve> eventListeners = new List<InflatableChangeEventCurve>();
    public List<InflatableMaterialFloatCurve> materialCurves = new List<InflatableMaterialFloatCurve>();

    [Tooltip("The amount of fluids that's considered neutral. A kobold might have 10 blood and shouldn't get any belly bulges from it, therefore this would need to be set to 10.")]
    public float defaultReagentVolume = 0f;
    [Tooltip("Consider this the scale of the object, bigger numbers mean less effect of fluid.")]
    public float reagentVolumeDivisor = 1f;
    [SerializeField]
    private GenericReagentContainer container;
    public void SetContainer(GenericReagentContainer newContainer) {
        if (container != null) {
            container.OnChange.RemoveListener(OnReagentContainerChanged);
        }
        StopAllCoroutines();
        container = newContainer;
        if (container != null) {
            container.OnChange.AddListener(OnReagentContainerChanged);
        }
        Start();
    }
    public GenericReagentContainer GetContainer() {
        return container;
    }
    public ScriptableReagent[] reagentMasks;
    private float currentSize = 0f;
    public AnimationCurve bounceCurve;
    //private AbstractGoTween tween;
    public float tweenDuration = 0.8f;
    private bool tweening = false;
    private float internalBaseSize = 0f;
    public float baseSize {
        get { return internalBaseSize; }
        set {
            internalBaseSize = value;
            TriggerTween();
        }
    }
    public float size {
        get {
            return currentSize;
        }
        set {
            //if (Mathf.Approximately(value, currentSize)) {
                //return;
            //}
            currentSize = value;
            foreach(var shapeCurve in shapeCurves) {
                float sample = shapeCurve.curve.Evaluate(currentSize);
                foreach (var renderer in targetRenderers) {
                    if (!shapeCurve.blendshapeIDs.ContainsKey(renderer)) {
                        for(int i=0;i<renderer.sharedMesh.blendShapeCount;i++) {
                            if (renderer.sharedMesh.GetBlendShapeName(i) == shapeCurve.blendshapeName) {
                                shapeCurve.blendshapeIDs[renderer] = i;
                            }
                        }
                    }
                    renderer.SetBlendShapeWeight(shapeCurve.blendshapeIDs[renderer], sample * 100f);
                }
            }
            foreach(var transformCurve in transformCurves) {
                float scaleSample = transformCurve.scaleCurve.Evaluate(currentSize);
                transformCurve.targetTransform.localScale = transformCurve.initialScale * scaleSample;
                if (!Mathf.Approximately(transformCurve.rotateAxis.magnitude,0f)) {
                    float rotateSample = transformCurve.rotateCurve.Evaluate(currentSize);
                    transformCurve.targetTransform.localRotation = Quaternion.AngleAxis(rotateSample, transformCurve.rotateAxis) * transformCurve.initialRotation;
                }
                if (!Mathf.Approximately(transformCurve.translationOffset.magnitude,0f)) {
                    float translateSample = transformCurve.translateOffsetCurve.Evaluate(currentSize);
                    transformCurve.targetTransform.localPosition = Vector3.LerpUnclamped(transformCurve.initialPosition, transformCurve.initialPosition + transformCurve.targetTransform.localRotation * transformCurve.translationOffset, translateSample);
                }
            }
            foreach (var softbodyCurve in softbodyCurves) {
                //float motionSample = softbodyCurve.motionFactorCurve.Evaluate(currentSize);
                //softbodyCurve.targetPhysics.zones[softbodyCurve.zoneIndex].amplitude = softbodyCurve.motionFactorDefault * motionSample;
                //float gravitySample = softbodyCurve.gravityFactorCurve.Evaluate(currentSize);
                //softbodyCurve.targetPhysics.zones[softbodyCurve.zoneIndex].gravity = softbodyCurve.gravityInOutDefault * gravitySample;
                float blendSample = softbodyCurve.blend.Evaluate(currentSize);
                (softbodyCurve.targetPhysics.jiggleZones[softbodyCurve.zoneIndex].jiggleSettings as JigglePhysics.JiggleSettingsBlend).normalizedBlend = blendSample;
                float radiusSample = softbodyCurve.radiusCurve.Evaluate(currentSize);
                softbodyCurve.targetPhysics.jiggleZones[softbodyCurve.zoneIndex].radius = softbodyCurve.radiusDefault * radiusSample;
            }
            foreach (var jiggleCurve in jiggleBoneCurves) {
                float blendSample = jiggleCurve.blend.Evaluate(currentSize);
                (jiggleCurve.targetRig.jiggleRigs[jiggleCurve.zoneIndex].jiggleSettings as JigglePhysics.JiggleSettingsBlend).normalizedBlend = blendSample;
            }
            foreach (var eventListener in eventListeners) {
                float eventSample = eventListener.curve.Evaluate(currentSize);
                eventListener.e.Invoke(eventSample);
            }
            foreach (var materialCurve in materialCurves) {
                float materialSample = materialCurve.curve.Evaluate(currentSize);
                foreach (Material m in materialCurve.cachedMaterials) {
                    m.SetFloat(materialCurve.floatPropertyName, materialSample);
                }
            }
        }
    }
    void Awake() {
        // Find all the blendshape IDs
        foreach(var inflatableShape in shapeCurves) {
            foreach (var renderer in targetRenderers) {
                inflatableShape.blendshapeIDs[renderer] = renderer.sharedMesh.GetBlendShapeIndex(inflatableShape.blendshapeName);
                if (inflatableShape.blendshapeIDs[renderer] == -1) {
                    Debug.LogError("Failed to find blendshape " + inflatableShape.blendshapeName + " on mesh " + renderer, this);
                }
            }
        }
        foreach (var transformCurve in transformCurves) {
            transformCurve.initialScale = transformCurve.targetTransform.localScale;
            transformCurve.initialRotation = transformCurve.targetTransform.localRotation;
            transformCurve.initialPosition = transformCurve.targetTransform.localPosition;
        }
        foreach( var inflatableSensor in softbodyCurves) {
            inflatableSensor.radiusDefault = inflatableSensor.targetPhysics.jiggleZones[inflatableSensor.zoneIndex].radius;
        }
        foreach (var materialCurve in materialCurves) {
            foreach (var r in materialCurve.targetRenderers) {
                materialCurve.cachedMaterials.Clear();
                foreach(Material m in r.materials) {
                    materialCurve.cachedMaterials.Add(m);
                }
            }
        }
    }
    void Start() {
        foreach( var inflatableSensor in softbodyCurves) {
            inflatableSensor.targetPhysics.jiggleZones[inflatableSensor.zoneIndex].jiggleSettings = JigglePhysics.JiggleSettingsBlend.Instantiate(inflatableSensor.targetPhysics.jiggleZones[inflatableSensor.zoneIndex].jiggleSettings);
        }
        foreach( var jiggleSensor in jiggleBoneCurves) {
            jiggleSensor.targetRig.jiggleRigs[jiggleSensor.zoneIndex].jiggleSettings = JigglePhysics.JiggleSettingsBlend.Instantiate(jiggleSensor.targetRig.jiggleRigs[jiggleSensor.zoneIndex].jiggleSettings);
        }
        size = GetDesiredSize();
    }
    void OnEnable() {
        StopAllCoroutines();
        tweening = false;
        if (container != null) {
            container.OnChange.AddListener(OnReagentContainerChanged);
        }
    }
    void OnDisable() {
        if (container != null) {
            size = GetDesiredSize();
            container.OnChange.RemoveListener(OnReagentContainerChanged);
        }
        tweening = false;
        StopAllCoroutines();
    }
    IEnumerator Tween(float duration) {
        float startTime = Time.timeSinceLevelLoad;
        float before = size;
        while (Time.timeSinceLevelLoad < startTime + duration) {
            float curve = bounceCurve.Evaluate((Time.timeSinceLevelLoad - startTime) / duration);
            size = Mathf.LerpUnclamped(before, GetDesiredSize(), curve);
            yield return null;
        }
        size = GetDesiredSize();
        tweening = false;
    }
    float GetDesiredSize() {
        if (container == null) {
            return baseSize/reagentVolumeDivisor;
        }
        float volume = baseSize;
        if (reagentMasks.Length == 0) {
            volume = container.volume + baseSize;
        } else {
            foreach (var mask in reagentMasks) {
                volume += container.GetVolumeOf(mask);
            }
        }
        volume -= defaultReagentVolume;
        return volume/reagentVolumeDivisor;
    }
    public void TriggerTween() {
        if (!isActiveAndEnabled || tweening) {
            return;
        }
        if (Mathf.Approximately(size, GetDesiredSize())) {
            return;
        }
        tweening = true;
        StartCoroutine(Tween(tweenDuration));
    }

    public void OnReagentContainerChanged(GenericReagentContainer.InjectType injectType) {
        TriggerTween();
    }
}
