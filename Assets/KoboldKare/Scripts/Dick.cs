using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Dick))]
public class DickEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Bake All")) {
            Undo.RecordObject(target, "Dick bake");
            ((Dick)target).BakeAll();
            EditorUtility.SetDirty(target);
        }
    }
    public void DrawWireDisk(float atLength, AnimationCurve xOffset, AnimationCurve yOffset, AnimationCurve girth, string label = "") {
        Dick d = (Dick)target;
        Vector3 offset = xOffset.Evaluate(atLength) * d.dickRoot.TransformDirection(d.dickRight) * d.dickRoot.TransformVector(d.dickRight).magnitude;
        offset += yOffset.Evaluate(atLength) * d.dickRoot.TransformDirection(d.dickUp) * d.dickRoot.TransformVector(d.dickUp).magnitude;
        offset += atLength * d.dickRoot.TransformDirection(d.dickForward) * d.dickRoot.TransformVector(d.dickForward).magnitude;
        float g = girth.Evaluate(atLength) * d.dickRoot.TransformVector(d.dickUp).magnitude;
        Handles.DrawWireDisc(d.dickRoot.position + offset, d.dickRoot.TransformDirection(d.dickForward), g*0.5f);
        if (!string.IsNullOrEmpty(label)) {
            Handles.Label(d.dickRoot.position + offset, label);
        }
    }
    public void OnSceneGUI() {
        if (Application.isPlaying) {
            return;
        }
        Transform t = (Transform)serializedObject.FindProperty("dickRoot").objectReferenceValue;
        //SerializedProperty dickOriginOffsetProp = serializedObject.FindProperty("dickOriginOffset");
        Vector3 dickForward = serializedObject.FindProperty("dickForward").vector3Value;
        Vector3 dickRight = serializedObject.FindProperty("dickRight").vector3Value;
        Vector3 dickUp = serializedObject.FindProperty("dickUp").vector3Value;
        var shapes = serializedObject.FindProperty("shapes");
        if (shapes != null && shapes.arraySize > 0) {
            AnimationCurve girth = shapes.GetArrayElementAtIndex(0).FindPropertyRelative("girth").animationCurveValue;
            AnimationCurve xOffset = shapes.GetArrayElementAtIndex(0).FindPropertyRelative("xOffset").animationCurveValue;
            AnimationCurve yOffset = shapes.GetArrayElementAtIndex(0).FindPropertyRelative("yOffset").animationCurveValue;
            SerializedProperty dickOriginOffsetProp = shapes.GetArrayElementAtIndex(0).FindPropertyRelative("localDickRoot");
            Handles.color = Color.white;
            if (girth.length <= 0) {
                return;
            }
            float shapeEnd = girth[girth.length - 1].time;
            //Vector3 tipOffset = t.TransformPoint(dickRight * xOffset.Evaluate(shapeEnd) + dickUp * yOffset.Evaluate(shapeEnd) + dickForward*shapeEnd);
            for(int i=0;i<girth.length-1;i++) {
                var keyOne = girth[i];
                var keyTwo = girth[i+1];
                Handles.color = Color.white;
                DrawWireDisk(keyOne.time, xOffset, yOffset, girth);
                Handles.color = Color.gray;
                DrawWireDisk(Mathf.Lerp(keyOne.time, keyTwo.time,0.5f), xOffset, yOffset, girth);
                Handles.color = Color.white;
                DrawWireDisk(keyTwo.time, xOffset, yOffset, girth);
            }
            var depthEvents = serializedObject.FindProperty("depthEvents");
            for(int i=0;i<depthEvents.arraySize;i++) {
                Handles.color = Color.blue;
                float length = girth.keys[girth.length-1].time;
                DrawWireDisk((1f-depthEvents.GetArrayElementAtIndex(i).FindPropertyRelative("triggerAlongDepth01").floatValue)*length, xOffset, yOffset, girth, "DepthEvent"+i);
            }
            if (t != null) {
                Vector3 globalPosition = Handles.PositionHandle(t.transform.TransformPoint(dickOriginOffsetProp.vector3Value), t.transform.rotation);
                if (Vector3.Distance(t.transform.InverseTransformPoint(globalPosition), dickOriginOffsetProp.vector3Value) > 0.001f) {
                    //Undo.RecordObject(target, "Dick origin move");
                    dickOriginOffsetProp.vector3Value = t.transform.InverseTransformPoint(globalPosition);
                    serializedObject.ApplyModifiedProperties();
                    //EditorUtility.SetDirty(target);
                }
                SerializedProperty enumProp = shapes.GetArrayElementAtIndex(0).FindPropertyRelative("shapeType");
                Handles.Label(t.transform.TransformPoint(dickOriginOffsetProp.vector3Value), enumProp.enumDisplayNames[enumProp.enumValueIndex] + " DICK ROOT");
                Dick d = (Dick)target;
                float g = d.GetWorldGirth(d.penetrationDepth01);
                Handles.color = Color.blue;
                Handles.DrawWireDisc(Vector3.zero, Vector3.up, g*0.5f);
            }
        }
    }
}
#endif

public class Dick : MonoBehaviour, IAdvancedInteractable, IFreezeReciever {
    public Rigidbody body {
        get {
            if (kobold == null || koboldBody == null) {
                return ragdollBody;
            }
            if (kobold.ragdolled) {
                return ragdollBody;
            } else {
                return koboldBody;
            }
        }
    }
    private ReagentContents dickCumContents = new ReagentContents();
    public int cumPulseCount = 12;
    public bool isDildo = true;
    public bool canOverpenetrate = true;
    public Rigidbody koboldBody;
    public Rigidbody ragdollBody;
    [Range(3, 128)]
    public int crossSections = 16;
    [HideInInspector]
    public Vector3 dickForward;
    [HideInInspector]
    public Vector3 dickUp;
    [HideInInspector]
    public Vector3 dickRight;
    public Kobold kobold;
    public List<Collider> selfColliders = new List<Collider>();
    private HashSet<Collider> ignoringCollisions = new HashSet<Collider>();
    [HideInInspector]
    public bool backwards;
    [HideInInspector]
    public float girthAtEntrance;
    [HideInInspector]
    public float girthAtExit;
    [Range(-1f,5f)]
    [SerializeField]
    private float internalPenetrationDepth01 = -1f;

    public float penetrationDepth01 {
        get {
            return internalPenetrationDepth01;
        }
        set {
            float diff = (value - internalPenetrationDepth01);
            float min = Mathf.Min(value, internalPenetrationDepth01);
            float max = Mathf.Max(value, internalPenetrationDepth01);
            foreach(DepthEvent de in depthEvents) {
                float triggerPoint = de.triggerAlongDepth01;
                if ((triggerPoint > min && triggerPoint < max)) {
                    if (de.triggerDirection == DepthEvent.TriggerDirection.Both ||
                     (de.triggerDirection == DepthEvent.TriggerDirection.PullOut && diff < 0) ||
                     (de.triggerDirection == DepthEvent.TriggerDirection.PushIn && diff > 0)) {
                        de.Trigger(dickRoot.position + dickRoot.TransformDirection(dickForward)*triggerPoint);
                    }
                }
            }
            internalPenetrationDepth01 = value;
        }
    }

    public Penetratable holeTarget;

    public List<AudioClip> pumpingSounds = new List<AudioClip>();
    public List<AudioClip> slimySlidingSounds = new List<AudioClip>();
    private bool dildoMemory = false;
    public GenericReagentContainer ballsContainer;
    public float cumVolumePerPump = 3f;
    public FluidOutput stream;
    [System.Serializable]
    public class DepthEvent {
        public enum TriggerDirection {
            Both,
            PushIn,
            PullOut
        }
        public void Trigger(Vector3 position) {
            if (lastTrigger != 0f && lastTrigger+triggerCooldown > Time.timeSinceLevelLoad) {
                return;
            }
            lastTrigger = Time.timeSinceLevelLoad;
            if (triggerClips.Count > 0) {
                GameManager.instance.SpawnAudioClipInWorld(triggerClips[Random.Range(0,triggerClips.Count)], position, soundTriggerVolume, GameManager.instance.soundEffectGroup);
            }
            triggerEvent.Invoke();
        }
        public float soundTriggerVolume = 1f;
        public float triggerCooldown = 1f;
        private float lastTrigger;
        public List<AudioClip> triggerClips = new List<AudioClip>();
        public TriggerDirection triggerDirection;
        [Range(0f,1f)]
        public float triggerAlongDepth01;
        public UnityEvent triggerEvent;
    }

    public List<DepthEvent> depthEvents = new List<DepthEvent>();

    public void SetHolePositions(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3) {
        this.p0 = p0;
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        orifaceNormal = Vector3.Normalize(-Bezier.BezierSlope(p0,p1,p2,p3,0f));
        orifaceOutNormal = Vector3.Normalize(-Bezier.BezierSlope(p0,p1,p2,p3,1f));
    }
    public bool invisibleWhenInside {
        set {
            foreach (var renderer in deformationTargets) {
                if (renderer == null) { continue; }
                Material[] materials = renderer.sharedMaterials;
                if (Application.isPlaying) {
                    materials = renderer.materials;
                }
                foreach (var material in materials) {
                    if (value) {
                        material.EnableKeyword("_INVISIBLE_WHEN_INSIDE_ON");
                    } else {
                        material.DisableKeyword("_INVISIBLE_WHEN_INSIDE_ON");
                    }
                }
            }
        }
    }
    public bool allTheWayThrough {
        set {
            foreach (var renderer in deformationTargets) {
                if (renderer == null) { continue; }
                Material[] materials = renderer.sharedMaterials;
                if (Application.isPlaying) {
                    materials = renderer.materials;
                }
                foreach (var material in materials) {
                    if (value) {
                        material.DisableKeyword("_CLIP_DICK_ON");
                    } else {
                        material.EnableKeyword("_CLIP_DICK_ON");
                    }
                }
            }
        }
    }
    private Vector3 p0 = Vector3.zero;
    private Vector3 p1 = Vector3.down*0.33f;
    private Vector3 p2 = Vector3.down*0.66f;
    private Vector3 p3 = Vector3.down;
    private Vector3 orifaceNormal = Vector3.up;
    private Vector3 orifaceOutNormal = Vector3.up;

    [Range(0f, 1f)]
    public float cumActive;
    [Range(-1f, 2f)]
    public float cumProgress;
    [Range(0.001f, 1f)]
    public float bulgePercentage = 0.1f;

    [System.Serializable]
    public class DickMoveEvent : UnityEvent<float>{}
    public UnityEvent OnPenetrate;
    public UnityEvent OnEndPenetrate;
    public DickMoveEvent OnMove;

    [Range(-1f,1f)]
    public float squishPullAmount = 0f;

    private DickShape internalDickshape = null;
    public DickShape defaultShape {
        get {
            if (internalDickshape != null && !Application.isEditor) {
                return internalDickshape;
            }
            foreach(DickShape shape in shapes) {
                if (shape.shapeType == DickShape.ShapeType.Default) {
                    internalDickshape = shape;
                    return shape;
                }
            }
            return internalDickshape;
        }
    }
    private Dictionary<Mesh, Matrix4x4> bindPoseCache = new Dictionary<Mesh, Matrix4x4>();
    private Matrix4x4 GetTransformPose(SkinnedMeshRenderer renderer) {
        if (bindPoseCache.ContainsKey(renderer.sharedMesh)) {
            return bindPoseCache[renderer.sharedMesh];
        }
        bindPoseCache.Add(renderer.sharedMesh, renderer.sharedMesh.bindposes[GetDickRootBoneID(renderer)]);
        return bindPoseCache[renderer.sharedMesh];
    }

    private int GetDickRootBoneID(SkinnedMeshRenderer renderer) {
        for(int i=0;i<renderer.bones.Length;i++) {
            if (renderer.bones[i] == dickRoot) { return i; }
        }
        return -1;
    }

    public Transform dickRoot;
    public Transform dickTip;
    private AudioSource slimySource;
    public List<SkinnedMeshRenderer> bakeTargets = new List<SkinnedMeshRenderer>();
    public List<SkinnedMeshRenderer> deformationTargets = new List<SkinnedMeshRenderer>();
    [System.Serializable]
    public class DickShape {
        public enum ShapeType {
            Default,
            Pull,
            Squish,
            Cum,
            Misc,
        }
        public float GetWeight(Dick d) {
            float weight = 0f;
            if (shapeType == DickShape.ShapeType.Misc && d.deformationTargets.Count > 0 && d.deformationTargets[0] != null) {
                weight = d.deformationTargets[0].GetBlendShapeWeight(blendshapeIDs[d.deformationTargets[0].sharedMesh]) / 100f;
            }
            if (shapeType == DickShape.ShapeType.Pull) {
                weight = Mathf.Clamp01(d.squishPullAmount);
            }
            if (shapeType == DickShape.ShapeType.Squish) {
                weight = Mathf.Clamp01(-d.squishPullAmount);
            }
            //if (shapeType == DickShape.ShapeType.Cum) {
                //weight = d.cumActive;
            //}
            return weight;
        }
        public float GetWeight(Dick d, float length) {
            float weight = 0f;
            if (shapeType == DickShape.ShapeType.Misc) {
                weight = d.deformationTargets[0].GetBlendShapeWeight(blendshapeIDs[d.deformationTargets[0].sharedMesh]) / 100f;
            }
            if (shapeType == DickShape.ShapeType.Pull) {
                weight = Mathf.Clamp01(d.squishPullAmount);
            }
            if (shapeType == DickShape.ShapeType.Squish) {
                weight = Mathf.Clamp01(-d.squishPullAmount);
            }
            if (shapeType == DickShape.ShapeType.Cum) {
                float fullLength = d.GetLocalLength();
                weight = d.cumActive * Easing.Circular.Out(1f-Mathf.Clamp01(Mathf.Abs(length - (d.cumProgress * fullLength)) / (fullLength * d.bulgePercentage)));
            }
            return weight;
        }
        public ShapeType shapeType;
        public string blendshapeName = "";
        [HideInInspector]
        public Dictionary<Mesh, int> blendshapeIDs = new Dictionary<Mesh, int>();
        public AnimationCurve xOffset = new AnimationCurve();
        public AnimationCurve yOffset = new AnimationCurve();
        public AnimationCurve girth = new AnimationCurve();
        public Vector3 localDickRoot;
    }
    public List<DickShape> shapes = new List<DickShape>();

    void Awake() {
        StartCoroutine(AwakeRoutine());
    }

    IEnumerator AwakeRoutine() {
        while (slimySource == null && Application.isPlaying) {
            slimySource = gameObject.AddComponent<AudioSource>();
            yield return null;
        }
        if (slimySource != null && GameManager.instance != null) {
            slimySource.outputAudioMixerGroup = GameManager.instance.soundEffectGroup;
            slimySource.rolloffMode = AudioRolloffMode.Logarithmic;
            slimySource.loop = true;
            slimySource.spatialBlend = 1f;
        }
    }

    void Start() {
        internalDickshape = null;
        if (deformationTargets.Count == 0) {
            return;
        }
        foreach(DickShape shape in shapes) {
            foreach (SkinnedMeshRenderer renderer in deformationTargets) {
                if (shape.blendshapeIDs.ContainsKey(renderer.sharedMesh)) {
                    shape.blendshapeIDs[renderer.sharedMesh] = renderer.sharedMesh.GetBlendShapeIndex(shape.blendshapeName);
                } else {
                    shape.blendshapeIDs.Add(renderer.sharedMesh, renderer.sharedMesh.GetBlendShapeIndex(shape.blendshapeName));
                }
            }
        }
    }
    public void OnDestroy() {
        if (slimySource != null) {
            Destroy(slimySource);
            slimySource = null;
        }
    }

    public virtual Vector3 GetLocalRootPosition() {
        if (defaultShape == null || defaultShape.girth.length == 0) {
            return Vector3.zero;
        }
        Vector3 baseRootPosition = defaultShape.localDickRoot;
        Vector3 rootPosition = baseRootPosition;
        foreach(DickShape shape in shapes) {
            float weight = shape.GetWeight(this);
            rootPosition += (shape.localDickRoot - baseRootPosition) * weight;
        }
        return rootPosition;
    }

    public virtual float GetLength() {
        return GetLocalLength() * dickRoot.TransformVector(dickForward).magnitude;
    }
    public virtual float GetLocalLength() {
        if (defaultShape == null || defaultShape.girth.length == 0) {
            return 1f;
        }
        float baseLength = defaultShape.girth[defaultShape.girth.length-1].time - defaultShape.girth[0].time;
        float length = baseLength;
        foreach(DickShape shape in shapes) {
            float weight = shape.GetWeight(this);
            float shapeLength = shape.girth[shape.girth.length - 1].time - shape.girth[0].time;
            length += (shapeLength - baseLength) * weight;
        }
        return length;
    }
    public virtual float GetTangent(float penetrationDepth01) {
        float fullLength = GetLocalLength();
        float localLength = (1f-penetrationDepth01) * fullLength;
        float baseTangent = 0f;
        float tangent = baseTangent;
        foreach(DickShape shape in shapes) {
            float weight = shape.GetWeight(this, localLength+shape.girth[0].time);
            tangent += (shape.girth.Differentiate(localLength+shape.girth[0].time) - baseTangent) * weight;
        }
        return tangent;
    }

    public virtual float GetWorldGirth(float penetrationDepth01) {
        float fullLength = GetLocalLength();
        float localLength = (1f-penetrationDepth01) * fullLength;
        float baseGirth = defaultShape.girth.Evaluate(localLength+defaultShape.girth[0].time);
        float girth = baseGirth;
        foreach(DickShape shape in shapes) {
            float weight = shape.GetWeight(this, localLength+shape.girth[0].time);
            girth += (shape.girth.Evaluate(localLength+shape.girth[0].time) - baseGirth) * weight;
        }
        return girth*dickRoot.TransformVector(dickUp).magnitude;
    }
    public virtual Vector3 GetWorldRootPosition() {
        return dickRoot.TransformPoint(GetLocalRootPosition());
    }
    public virtual Vector3 GetWorldPlanarOffset(float penetrationDepth01) {
        if (defaultShape == null || defaultShape.xOffset.length <=0) {
            return Vector3.zero;
        }
        float fullLength = GetLocalLength();
        float localLength = (1f-penetrationDepth01) * fullLength;
        Vector3 baseOffset = dickRight * defaultShape.xOffset.Evaluate(localLength+defaultShape.xOffset[0].time) +
                             dickUp * defaultShape.yOffset.Evaluate(localLength+defaultShape.yOffset[0].time);
        Vector3 offset = baseOffset;
        foreach(DickShape shape in shapes) {
            float weight = shape.GetWeight(this, localLength+shape.girth[0].time);
            Vector3 evalOffset = dickRight * shape.xOffset.Evaluate(localLength+shape.xOffset[0].time) +
                                 dickUp * shape.yOffset.Evaluate(localLength+shape.yOffset[0].time);
            offset += (evalOffset - baseOffset) * weight;
        }
        return dickRoot.TransformVector(offset);
    }

#if UNITY_EDITOR
    void BakeShape(DickShape shape) {
        shape.girth = new AnimationCurve();
        shape.xOffset = new AnimationCurve();
        shape.yOffset = new AnimationCurve();
        List<Vector3> dickVerts = new List<Vector3>();
        foreach(SkinnedMeshRenderer skinnedMesh in bakeTargets) {
            Mesh mesh = skinnedMesh.sharedMesh;
            List<Vector3> verts = new List<Vector3>();
            mesh.GetVertices(verts);

            // Apply blendshape
            if (!string.IsNullOrEmpty(shape.blendshapeName)) {
                Vector3[] blendVerts = new Vector3[mesh.vertexCount];
                Vector3[] blendNormals = new Vector3[mesh.vertexCount];
                Vector3[] blendTangents = new Vector3[mesh.vertexCount];
                mesh.GetBlendShapeFrameVertices(shape.blendshapeIDs[mesh], 0, blendVerts, blendNormals, blendTangents);
                for (int i = 0; i < verts.Count; i++) {
                    verts[i] += blendVerts[i];
                }
            }

            // This weird junk is to iterate through every weight of every vertex.
            // Weights aren't limited to 4 bones, they go up to bonesPerVertex[i] where i is the vertex index in question.
            // Since even each vertex could have any number of bones, I just keep a separate vertex incrementer (vt) and 
            // a weight incrementer (wt).
            var weights = mesh.GetAllBoneWeights();
            var bonesPerVertex = mesh.GetBonesPerVertex();
            int vt = 0;
            int wt = 0;
            for (int o = 0; o < bonesPerVertex.Length; o++) {
                for (int p = 0; p < bonesPerVertex[o]; p++) {
                    BoneWeight1 weight = weights[wt];
                    Transform boneWeightTarget = skinnedMesh.bones[weights[wt].boneIndex];
                    if (boneWeightTarget.IsChildOf(dickRoot) && weights[wt].weight > 0f) {
                        dickVerts.Add(mesh.bindposes[GetDickRootBoneID(skinnedMesh)].MultiplyPoint(verts[vt]));
                    }
                    wt++;
                }
                vt++;
            }
        }
        if (dickVerts.Count <= 0) {
            throw new UnityException("There was no dick verts found weighted to the target transform or its children! Make sure they have a weight assigned in the mesh.");
        }
        // Sort them front to back, based on the dickForward axis.
        dickVerts.Sort((a, b) => Vector3.Dot(a, dickForward).CompareTo(Vector3.Dot(b, dickForward)));
        float start = Vector3.Dot(dickVerts[0], dickForward);
        float end = Vector3.Dot(dickVerts[dickVerts.Count-1], dickForward);
        float length = end - start;
        float crossSectionLength = length / (float)(crossSections-1);
        float targetPlane = start;
        // Split them into cross sections, and analyze each one for the girth and x/y offset.
        for (int crossSectionNumber = 0;crossSectionNumber<crossSections;crossSectionNumber++) {
            // Get all the verts closest to the plane
            dickVerts.Sort((a, b) => Mathf.Abs(Vector3.Dot(a, dickForward)-targetPlane).CompareTo(Mathf.Abs(Vector3.Dot(b, dickForward)-targetPlane)));
            List<Vector3> crossSection = new List<Vector3>();
            for(int i = 0; i < (dickVerts.Count / crossSections); i++) {
                crossSection.Add(dickVerts[i]);
            }
            crossSection.Sort((a, b) => Vector3.Dot(a, dickRight).CompareTo(Vector3.Dot(b, dickRight)));
            float crossWidth = Vector3.Dot(crossSection[crossSection.Count - 1], dickRight) - Vector3.Dot(crossSection[0], dickRight);
            float crossRightCenter = Vector3.Dot(crossSection[0], dickRight) + crossWidth / 2f;
            crossSection.Sort((a, b) => Vector3.Dot(a, dickUp).CompareTo(Vector3.Dot(b, dickUp)));
            float crossHeight = Vector3.Dot(crossSection[crossSection.Count - 1], dickUp) - Vector3.Dot(crossSection[0], dickUp);
            float crossHeightCenter = Vector3.Dot(crossSection[0], dickUp) + crossHeight / 2f;
            if (crossSectionNumber == 0 || crossSectionNumber == crossSections - 1) {
                // We always gotta end and start at 0.
                shape.girth.AddKey(new Keyframe(targetPlane, 0f));
            } else {
                shape.girth.AddKey(new Keyframe(targetPlane, ((crossWidth + crossHeight) * 0.5f)));
            }
            shape.xOffset.AddKey(new Keyframe(targetPlane, crossRightCenter));
            shape.yOffset.AddKey(new Keyframe(targetPlane, crossHeightCenter));
            targetPlane += crossSectionLength;
        }
        // Instead of offseting the localDickRoot on the X/Y plane, we keep it perfectly aligned with the root bone.
        // This is actually a requirement since the shader doesn't account for X/Y offets at all.
        shape.localDickRoot = dickForward * start;

        // Just make sure the graph is smooth. ClampForever makes things sampling too far away get a girth/offset of 0.
        shape.girth.AutoSmooth();
        shape.girth.preWrapMode = WrapMode.ClampForever;
        shape.girth.postWrapMode = WrapMode.ClampForever;
        shape.xOffset.AutoSmooth();
        shape.xOffset.preWrapMode = WrapMode.ClampForever;
        shape.xOffset.postWrapMode = WrapMode.ClampForever;
        shape.yOffset.AutoSmooth();
        shape.yOffset.preWrapMode = WrapMode.ClampForever;
        shape.yOffset.postWrapMode = WrapMode.ClampForever;
    }
    public void BakeAll() {
        Start();
        dickForward = dickRoot.InverseTransformDirection((dickTip.position - dickRoot.position).normalized);
        if (Vector3.Dot(dickForward, dickRoot.up) > 0.9f) {
            // if the dick root is y forward, then we should use z forward instead.
            dickUp = dickRoot.forward;
        } else {
            // Otherwise up should work fine.
            dickUp = dickRoot.up;
        }
        dickRight = Vector3.Cross(dickForward, dickUp);
        Vector3.OrthoNormalize(ref dickForward, ref dickUp, ref dickRight);
        foreach(DickShape shape in shapes) {
            BakeShape(shape);
        }
    }
#endif
    public void SetDeforms() {
        if (dickRoot == null) {
            return;
        }
        float dickLength = GetLength();
        float orifaceLength = 1f;
        if (holeTarget != null) {
            orifaceLength = holeTarget.orifaceLength;
        }
        if (dickRoot != null && deformationTargets.Count > 0) {
            foreach (var renderer in deformationTargets) {
                if (renderer == null) {
                    continue;
                }
                Material[] materials = renderer.sharedMaterials;
                if (Application.isPlaying) {
                    materials = renderer.materials;
                }
                foreach (var material in materials) {
                    material.SetVector("_DickOrigin", Vector3.Scale(renderer.rootBone.worldToLocalMatrix.MultiplyPoint(dickRoot.TransformPoint(GetLocalRootPosition())), renderer.rootBone.lossyScale));
                    material.SetVector("_DickForward", Vector3.Normalize(renderer.rootBone.worldToLocalMatrix.MultiplyVector(dickRoot.TransformDirection(dickForward))));
                    material.SetVector("_DickRight", Vector3.Normalize(renderer.rootBone.worldToLocalMatrix.MultiplyVector(dickRoot.TransformDirection(dickRight))));
                    material.SetVector("_DickUp", Vector3.Normalize(renderer.rootBone.worldToLocalMatrix.MultiplyVector(dickRoot.TransformDirection(dickUp))));
                    material.SetFloat("_DickLength", dickLength);
                    if (penetrationDepth01 < 0f) {
                        material.SetFloat("_PenetrationDepth", -(1f-Easing.Exponential.In(Mathf.Clamp01(1f+penetrationDepth01))));
                    } else {
                        material.SetFloat("_PenetrationDepth", penetrationDepth01);
                    }
                    material.SetVector("_OrifaceWorldNormal", orifaceNormal);
                    material.SetVector("_OrifaceOutWorldNormal", orifaceOutNormal);
                    material.SetVector("_OrifaceWorldPosition", p0-GetWorldPlanarOffset(penetrationDepth01));
                    material.SetVector("_OrifaceOutWorldPosition1", p1);
                    material.SetVector("_OrifaceOutWorldPosition2", p2);
                    material.SetVector("_OrifaceOutWorldPosition3", p3-GetWorldPlanarOffset(penetrationDepth01-(orifaceLength/dickLength)));
                    material.SetFloat("_OrifaceLength", orifaceLength);
                    material.SetFloat("_BlendshapeMultiplier", dickRoot.TransformVector(dickUp).magnitude * GetTransformPose(renderer).lossyScale.x);
                    material.SetFloat("_CumActive", cumActive);
                    material.SetFloat("_CumProgress", cumProgress);
                    material.SetFloat("_BulgePercentage", bulgePercentage);
                    material.SetFloat("_SquishPullAmount", squishPullAmount);
                }
            }
        }
    }
    public void OnTriggerEnter(Collider collider) {
        CheckCollision(collider);
    }
    public void OnTriggerStay(Collider collider) {
        CheckCollision(collider);
    }
    public void IgnoreCollision(Kobold k) {
        if (k == null || k == kobold) {
            return;
        }
        foreach (Collider d in selfColliders) {
            //if (d.gameObject.layer == LayerMask.NameToLayer("PlayerHitbox")) {
            //d.gameObject.layer = LayerMask.NameToLayer("Player");
            //}
            foreach (Collider e in k.GetComponentsInChildren<Collider>()) {
                Physics.IgnoreCollision(e, d, true);
                ignoringCollisions.Add(e);
            }
        }
    }
    public void UnignoreAll() {
        foreach (Collider d in selfColliders) {
            if (d == null) {
                continue;
            }
            //if (d.gameObject.layer == LayerMask.NameToLayer("Player")) {
            //d.gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
            //}
            foreach (Collider c in ignoringCollisions) {
                if (c == null) {
                    continue;
                }
                Physics.IgnoreCollision(c, d, false);
            }
        }
        ignoringCollisions.Clear();
    }
    public void CheckCollision(Collider collider) {
        if (!isActiveAndEnabled) {
            return;
        }
        //if (!grabbed) {
        //return;
        //}
        Kobold k = collider.gameObject.GetComponentInParent<Kobold>();
        if (k == null || k == kobold) {
            return;
        }
        // Don't penetrate kobolds that are penetrating us!
        //if (k.activeDicks.Count > 0 && k.activeDicks[0].dick.holeTarget != null && kobold == k.activeDicks[0].dick.holeTarget.transform.root.GetComponent<Kobold>()) {
            //return;
        //}
        // Don't penetrate a kobold that's already penetrating us
        //if (k.activeDicks.Count > 0 && k.activeDicks[0].dick.holeTarget != null && k.activeDicks[0].dick.holeTarget.transform.root.GetComponent<Kobold>() == kobold) {
            //return;
        //}
        if (holeTarget != null) {
            return;
        }
        Penetratable closestPenetratable = collider.GetComponentInParent<Penetratable>();
        if (closestPenetratable != null) {
            float dist = Vector3.Distance(transform.position, closestPenetratable.path[0].position);
            float dist2 = Vector3.Distance(transform.position, closestPenetratable.path[3].position);
            backwards = (dist2 < dist && closestPenetratable.canAllTheWayThrough);
            if (backwards) {
                dist = dist2;
            }
            float angleDiff = Vector3.Dot(closestPenetratable.GetTangent(0f, backwards), dickRoot.TransformDirection(dickForward));
            if (!closestPenetratable.ContainsPenetrator(this) && angleDiff > -0.25f) {
                if (dist > GetLength()) {
                    return;
                }
                if (isDildo) {
                    body.isKinematic = true;
                }
                IgnoreCollision(k);
                //dick.body.maxAngularVelocity = 64f;
                penetrationDepth01 = 0f;
                holeTarget = closestPenetratable;
                invisibleWhenInside = !holeTarget.canSeeDickInside;
                allTheWayThrough = holeTarget.canAllTheWayThrough;
                if (slimySlidingSounds.Count > 0) {
                    slimySource.clip = slimySlidingSounds[Random.Range(0,slimySlidingSounds.Count)];
                    slimySource.volume = 0f;
                    slimySource.Play();
                }
                holeTarget.AddPenetrator(this);
                OnPenetrate.Invoke();
            }
        }
    }
    public void OnValidate() {
        Start();
        SetDeforms();
    }

    void Update() {
        if (Application.isEditor && !Application.isPlaying) {
            SetDeforms();
            return;
        }
        if (slimySource != null) {
            slimySource.volume = Mathf.MoveTowards(slimySource.volume, 0f, Time.deltaTime*0.4f);
        }
        if (holeTarget != null) {
            //penetrationDepth01 += GetTangent(penetrationDepth01)*Time.deltaTime;
            //penetrationDepth01 += GetTangent(penetrationDepth01-(holeTarget.orifaceLength/GetLength()))*Time.deltaTime;
        } else {
            penetrationDepth01 = -1f;
        }
        if (!isDildo && holeTarget != null) {
            interactPointOffset = 0f;
            interactPointSet = true;
            InteractTo(GetWorldRootPosition(), body.rotation);
        }
        SetDeforms();
    }
    void FixedUpdate() {
        if (selfColliders.Count > 0 && selfColliders[0] is CapsuleCollider) {
            selfColliders[0].transform.localScale = (Vector3.one - dickForward*Mathf.Clamp01(penetrationDepth01));
        }
    }


// KoboldKare specific stuff
    private bool interactPointSet = false;
    private float interactPointOffset;
    public void InteractTo(Vector3 worldPosition, Quaternion worldRotation) {
        if (holeTarget == null){
            return;
        }
        if (interactPointSet == false) {
            interactPointSet = true;
            interactPointOffset = (Vector3.Dot(dickForward,dickRoot.InverseTransformPoint(worldPosition)))/GetLocalLength() - Vector3.Dot(dickForward,GetLocalRootPosition());
        }
        float length = GetLength();
        // Calculate where the "first" shape is located along the oriface path.
        float firstShapeOffset = ((backwards?1f-holeTarget.shapes[holeTarget.shapes.Count-1].alongPathAmount01:holeTarget.shapes[0].alongPathAmount01)*holeTarget.orifaceLength)/length;
        float lastShapeOffset = ((backwards?holeTarget.shapes[0].alongPathAmount01:1f-holeTarget.shapes[holeTarget.shapes.Count-1].alongPathAmount01)*holeTarget.orifaceLength)/length;
        // If we cannot overpenetrate, we use a method that simply uses the distance to the hole to determine how deep we are.
        if (!canOverpenetrate) {
            float dist = Vector3.Distance(worldPosition, holeTarget.GetPoint(0,backwards))+(interactPointOffset*length);
            float diff = ((1f-(dist/GetLength()))-penetrationDepth01);
            // Start squishing or pulling based purely on the distance to the crotch
            squishPullAmount = Mathf.MoveTowards(squishPullAmount, 0f, Time.deltaTime);
            squishPullAmount -= diff*Time.deltaTime*40f;
            squishPullAmount = Mathf.Clamp(squishPullAmount, -1f, 1f);
            // If we're fully squished or pulled, we finally start sliding.
            if (Mathf.Abs(squishPullAmount) == 1f) {
                float move = diff*Time.deltaTime*20f*Easing.Cubic.Out(Mathf.Abs(squishPullAmount));
                // Calculate the tangents, which is used for knot forces at both the entrance and exit shape.
                float girthTangents = GetTangent(penetrationDepth01 - firstShapeOffset);
                girthTangents += GetTangent(penetrationDepth01-(holeTarget.orifaceLength/length) + lastShapeOffset);

                // If we're working against a knot force, don't slide
                if (girthTangents * move < 0) {
                    move *= Mathf.Clamp(1f-Mathf.Abs(girthTangents), 0.5f, 1f);
                }
                slimySource.volume = Mathf.Clamp01(Mathf.Abs(move*10f*Time.deltaTime*10f)+slimySource.volume);
                
                OnMove.Invoke(move);
                penetrationDepth01 = Mathf.Clamp(penetrationDepth01+move, -1f, 1f);
            }
        // Otherwise, we use a moving plane that follows the normal of the oriface path, and use the plane distance to the desired point to determine which way we should go.
        } else {
            float orifaceDepth01 = ((penetrationDepth01-1f)+interactPointOffset)*GetLength()/holeTarget.orifaceLength;
            Vector3 holePos = holeTarget.GetPoint(orifaceDepth01, backwards);
            Vector3 holeTangent = holeTarget.GetTangent(orifaceDepth01, backwards).normalized;
            Vector3 holeToMouse = worldPosition - holePos;
            //squishPullAmount = Mathf.MoveTowards(squishPullAmount, 0f, Time.deltaTime);
            squishPullAmount -= Vector3.Dot(holeToMouse, holeTangent)*Time.deltaTime*10f;
            squishPullAmount = Mathf.Clamp(squishPullAmount, -1f, 1f);
            if (Mathf.Abs(squishPullAmount) == 1f) {
                float move = Vector3.Dot(holeToMouse, holeTangent)*Time.deltaTime*8f;
                float girthTangents = GetTangent(penetrationDepth01 - firstShapeOffset);
                girthTangents += GetTangent(penetrationDepth01-(holeTarget.orifaceLength/length) + lastShapeOffset);
                if (girthTangents * move < 0) {
                    move *= Mathf.Clamp(1f-Mathf.Abs(girthTangents*0.7f), 0.025f, 1f);
                }
                slimySource.volume = Mathf.Clamp01(Mathf.Abs(move*10f*Time.deltaTime*10f)+slimySource.volume);
                OnMove.Invoke(move);
                penetrationDepth01 = Mathf.Max(penetrationDepth01+move, -1f);
            }
        }
        // Prevent the dick from penetrating futher than intended.
        if (!holeTarget.canAllTheWayThrough) {
            penetrationDepth01 = Mathf.Min(penetrationDepth01,holeTarget.allowedPenetrationDepth01*holeTarget.orifaceLength/GetLength());
        }
        float rootTargetPoint = (penetrationDepth01-1f)*GetLength()/holeTarget.orifaceLength;
        if (rootTargetPoint > 1f || penetrationDepth01 < -0.25f) {
            OnEndInteract(null);
        }
    }
    public void Cum() {
        StopCoroutine("CumRoutine");
        StartCoroutine("CumRoutine");
    }

    public IEnumerator CumRoutine() {
        cumActive = 1f;
        for(int i=0;i<cumPulseCount;i++) {
            cumProgress = -bulgePercentage;
            if (pumpingSounds.Count > 0) {
                GameManager.instance.SpawnAudioClipInWorld(pumpingSounds[Random.Range(0,pumpingSounds.Count)], transform.position, 1f, GameManager.instance.soundEffectGroup);
            }
            while (cumProgress < 1f+bulgePercentage) {
                cumProgress = Mathf.MoveTowards(cumProgress, 1f+bulgePercentage, Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            dickCumContents.Mix(ReagentData.ID.Cum, dickRoot.TransformVector(dickRight).magnitude*0.1f);
            dickCumContents.Mix(ballsContainer.contents.Spill(cumVolumePerPump*dickRoot.TransformVector(dickRight).magnitude));
            // Just add a little extra cum just in case the balls are empty
            if (holeTarget != null && holeTarget.connectedContainer != null && penetrationDepth01*GetLength() < holeTarget.orifaceLength && penetrationDepth01 > 0f) {
                holeTarget.connectedContainer.contents.Mix(dickCumContents);
            } else {
                stream.Fire(dickCumContents, cumVolumePerPump/10f);
            }
        }
        cumActive = 0f;
    }
    public void OnFreeze(Kobold k) {
        dildoMemory = isDildo;
        isDildo = false;
    }

    public void OnEndFreeze() {
        isDildo = dildoMemory;
    }
    public void OnInteract(Kobold k) {
        interactPointSet = false;
    }

    public void OnEndInteract(Kobold k) {
        if (holeTarget == null) {
            return;
        }
        slimySource.Stop();
        float rootTargetPoint = (penetrationDepth01-1f)*GetLength()/holeTarget.orifaceLength;
        if (penetrationDepth01<0f || rootTargetPoint > 1f) {
            interactPointSet = false;
            penetrationDepth01 = -1f;
            squishPullAmount = 0f;
            SetDeforms();
            holeTarget.RemovePenetrator(this);
            OnEndPenetrate.Invoke();
            holeTarget = null;
            UnignoreAll();
            body.isKinematic = false;
            body.drag = 0f;
            body.useGravity = true;
            body.angularDrag = 0.05f;
        }
    }
    public bool PhysicsGrabbable() {
        return true;
    }
}
