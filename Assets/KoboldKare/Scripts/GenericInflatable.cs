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
        public JigglePhysics.JiggleBone jiggleBone;
        [HideInInspector]
        public Vector3 initialScale;
        [HideInInspector]
        public Quaternion initialRotation;
        [HideInInspector]
        public Vector3 initialPosition;
    }
    [System.Serializable]
    public class InflatableSoftbody {
        public JigglePhysics.JiggleSoftbody targetPhysics;
        public int zoneIndex;
        public AnimationCurve motionFactorCurve;
        public AnimationCurve gravityFactorCurve;
        public AnimationCurve radiusCurve;
        [HideInInspector]
        public float motionFactorDefault;
        [HideInInspector]
        public Vector2 gravityInOutDefault;
        [HideInInspector]
        public float radiusDefault;
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
                    if (transformCurve.jiggleBone == null) {
                        transformCurve.targetTransform.localRotation = Quaternion.AngleAxis(rotateSample, transformCurve.rotateAxis) * transformCurve.initialRotation;
                    } else {
                        transformCurve.jiggleBone.GetVirtualBone(transformCurve.targetTransform).localStartRot = Quaternion.AngleAxis(rotateSample, transformCurve.rotateAxis) * transformCurve.initialRotation;
                    }
                }
                if (!Mathf.Approximately(transformCurve.translationOffset.magnitude,0f)) {
                    float translateSample = transformCurve.translateOffsetCurve.Evaluate(currentSize);
                    if (transformCurve.jiggleBone == null) {
                        transformCurve.targetTransform.localPosition = Vector3.LerpUnclamped(transformCurve.initialPosition, transformCurve.initialPosition + transformCurve.targetTransform.localRotation * transformCurve.translationOffset, translateSample);
                    } else {
                        JigglePhysics.JiggleBone.VirtualBone b = transformCurve.jiggleBone.GetVirtualBone(transformCurve.targetTransform);
                        b.localStartPos = Vector3.LerpUnclamped(transformCurve.initialPosition, transformCurve.initialPosition + transformCurve.targetTransform.localRotation * transformCurve.translationOffset, translateSample);
                    }
                }
            }
            foreach (var softbodyCurve in softbodyCurves) {
                float motionSample = softbodyCurve.motionFactorCurve.Evaluate(currentSize);
                softbodyCurve.targetPhysics.zones[softbodyCurve.zoneIndex].amplitude = softbodyCurve.motionFactorDefault * motionSample;
                float gravitySample = softbodyCurve.gravityFactorCurve.Evaluate(currentSize);
                softbodyCurve.targetPhysics.zones[softbodyCurve.zoneIndex].gravity = softbodyCurve.gravityInOutDefault * gravitySample;
                float radiusSample = softbodyCurve.radiusCurve.Evaluate(currentSize);
                softbodyCurve.targetPhysics.zones[softbodyCurve.zoneIndex].radius = softbodyCurve.radiusDefault * radiusSample;
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
            transformCurve.jiggleBone = null;
            foreach(JigglePhysics.JiggleBone bone in transformCurve.targetTransform.root.GetComponentsInChildren<JigglePhysics.JiggleBone>()) {
                if (bone.IsSimulatingBone(transformCurve.targetTransform)) {
                    transformCurve.jiggleBone = bone;
                    break;
                }
            }
        }
        foreach( var inflatableSensor in softbodyCurves) {
            inflatableSensor.radiusDefault = inflatableSensor.targetPhysics.zones[inflatableSensor.zoneIndex].radius;
            inflatableSensor.gravityInOutDefault = inflatableSensor.targetPhysics.zones[inflatableSensor.zoneIndex].gravity;
            inflatableSensor.motionFactorDefault = inflatableSensor.targetPhysics.zones[inflatableSensor.zoneIndex].amplitude;
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
        if (container == null) {
            return;
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
            yield return waitForEndOfFrame;
        }
        size = GetDesiredSize();
        tweening = false;
    }
    float GetDesiredSize() {
        float volume = 0f;
        if (reagentMasks.Length == 0) {
            volume = container.volume;
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
