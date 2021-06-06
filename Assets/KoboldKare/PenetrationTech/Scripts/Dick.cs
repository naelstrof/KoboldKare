using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Naelstrof {
    public class Dick : MonoBehaviour {
        public float arousal { get; set; } = 1f;
        [System.Serializable]
        public class UnityEventFloat : UnityEvent<float> { };
        [Range(2, 128)]
        public static string[] blendshapeTypeName = {
            "None",
            "Squish",
            "Pull",
            "Cum"
        };
        public enum BlendshapeType {
            None = 0,
            Squish,
            Pull,
            Cum
        }
        public static string BlendshapeTypeToString(BlendshapeType a) {
            return blendshapeTypeName[(int)a];
        }
        [HideInInspector]
        public List<int> blendshapeIDs = new List<int>();
        public Vector3 dickForwardAxis = new Vector3(0, 1, 0);
        public Vector3 dickUpAxis = new Vector3(0, 0, -1);
        public Vector3 dickRightAxis = new Vector3(1, 0, 0);

        [Tooltip("This is the transform that can point the dick toward holes. It's really important for both baking and runtime for this to be set correctly.")]
        public Transform dickTransform;
        //[HideInInspector]
        public List<AnimationCurve> girthCurves = new List<AnimationCurve>();
        //[HideInInspector]
        public List<AnimationCurve> xOffsetCurves = new List<AnimationCurve>();
        //[HideInInspector]
        public List<AnimationCurve> yOffsetCurves = new List<AnimationCurve>();
        public List<SkinnedMeshRenderer> bakeMeshes;
        public List<SkinnedMeshRenderer> deformationTargets = new List<SkinnedMeshRenderer>();
        [Range(0f, 10f)]
        [Tooltip("This variable adjusts how large of an area around the hole is affected by squish/pull/cum effects. Adjust it until only an area around the dick during deformation is affected to taste.")]
        public float blendshapeSoftness = 1f;
        public Naelstrof.Penetratable holeTarget;
        public Material strandMaterial;
        public StreamRenderer stream;
        public GenericReagentContainer balls;
        public Transform hitBoxCollider;
        [Range(-1f, 2f)]
        public float cumProgress = 0f;
        private float pushPullLerper;
        public float cumProgressProperty {
            get {
                return cumProgress;
            }
            set {
                cumProgress = value;
            }
        }


        [Range(0f, 1f)]
        public float cumActive = 0f;
        private float lastDistance;
        private Matrix4x4 bindPoseDickTransformCache;
        [HideInInspector]
        public List<float> weights = new List<float>();
        private List<float> lengths = new List<float>();
        private List<float> minPen = new List<float>();
        private int strandCount = 3;
        private List<Strand> strands = new List<Strand>();
        private List<Transform> randomSurfacePoints = new List<Transform>();
        public UnityEvent OnPenetrate;
        public UnityEvent PenetrateContinuous;
        public UnityEvent OnDepenetrate;
        public UnityEventFloat OnMove;
        [HideInInspector]
        public float aimWeight;
        private bool penetrating = false;
        public Rigidbody body;
        public List<AudioClip> pumpingSounds = new List<AudioClip>();
        public List<AudioClip> plappingSounds = new List<AudioClip>();
        public AudioClip slidingSound;
        private AudioSource slidingSoundSource;
        private bool playedPlap = false;
        private float pushPullAmount = 0f;
        private List<Material> materials = new List<Material>();
        public Kobold kobold;
        public float girthForceMultiplier = 1f;

        public IEnumerator CumProcedure(float duration, int pulses, float delay) {
            cumActive = 1f;
            cumProgress = -0.2f;
            for (int i = 0; i < pulses; i++) {
                // Start pulse
                balls.contents.Mix(ReagentData.ID.Cum, dickTransform.lossyScale.x * 0.5f);
                kobold?.PumpUpDick(1f);
                GameManager.instance.SpawnAudioClipInWorld(pumpingSounds[UnityEngine.Random.Range(0, pumpingSounds.Count)], transform.position, Mathf.Clamp01(transform.lossyScale.x * 0.2f));

                // Do pulse
                float startTime = Time.timeSinceLevelLoad;
                while (Time.timeSinceLevelLoad - startTime < duration) {
                    float progress = (Time.timeSinceLevelLoad - startTime) / duration;
                    // Ranges from -0.2f to 1.2f, to make sure we don't get some bulge at the start and end of the dick.
                    cumProgressProperty = -0.2f + progress * 1.4f;
                    yield return new WaitForEndOfFrame();
                }

                // End of pulse
                stream.radius = (Mathf.Sqrt(dickTransform.lossyScale.x + 1f) - 1f) * 0.03f;
                if (holeTarget != null && holeTarget.connectedContainer != null && aimWeight > 0.9f) {
                    GenericReagentContainer container = holeTarget.connectedContainer.GetComponent<GenericReagentContainer>();
                    if (container != null) {
                        Vector3 cumAxis = holeTarget.holeTransform.TransformDirection(holeTarget.holeForwardAxis);
                        ReagentContents fluidTransfer = balls.contents.FilterFluids(dickTransform.lossyScale.x * 0.5f, GameManager.instance.reagentDatabase);
                        if (fluidTransfer.volume > 0f) {
                            GameManager.instance.SpawnDecalInWorld(stream.splashMaterial, dickTransform.position + dickTransform.TransformDirection(dickForwardAxis) * GetWorldLength() + cumAxis * 0.25f, -cumAxis, Vector2.one * dickTransform.lossyScale.x, fluidTransfer.GetColor(GameManager.instance.reagentDatabase), dickTransform.gameObject, 0.5f, false);
                            GameManager.instance.SpawnDecalInWorld(stream.splashMaterial, holeTarget.GetSamplePosition() - cumAxis * 0.25f, cumAxis, Vector2.one * dickTransform.lossyScale.x, fluidTransfer.GetColor(GameManager.instance.reagentDatabase), holeTarget.gameObject, 0.5f);
                            container.contents.Mix(fluidTransfer);
                        }
                    }
                    //Rigidbody body = holeTarget.GetComponentInParent<Rigidbody>();
                    //if ( body != null ) {
                    //body.AddForceAtPosition(dickTransform.TransformDirection(dickForwardAxis) * Mathf.Min(dickTransform.lossyScale.x, 5f), dickTransform.position);
                    //}
                    stream.StopFiring();
                } else {
                    stream.Fire(balls, dickTransform.lossyScale.x * 0.8f);
                }
                yield return new WaitForSeconds(delay);
            }
            cumActive = 0f;
        }
        public void AddSlideForce(float distance) {
            float localDistance = (distance / (dickTransform.lossyScale.x*2f)) * 10f;
            OnMove.Invoke(Mathf.Abs(localDistance));
            kobold?.AddStimulation(Mathf.Abs(localDistance));
            pushPullAmount = Mathf.Clamp(pushPullAmount + localDistance, -1f, 1f);
        }
        public void Cum() {
            if (!isActiveAndEnabled) {
                return;
            }
            StopAllCoroutines();
            StartCoroutine(CumProcedure(1.6f, 12, 0.25f));
        }
        public void Start() {
            materials.Clear();
            foreach (SkinnedMeshRenderer r in deformationTargets) {
                materials.AddRange(r.materials);
            }
        }
        public Vector3 GetRandomPointOnSurface(float height) {
            Vector3 strandPos = GetXYZOffsetWorld(height, weights);
            strandPos += Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, dickTransform.TransformDirection(dickForwardAxis)).normalized * GetGirthWorld(height, weights) * 0.4f;
            return strandPos + dickTransform.position;
        }
        public float GetHeightSample(float worldDistance) {
            return dickTransform.InverseTransformPoint(worldDistance * dickUpAxis + dickTransform.position).magnitude;
        }
        public float GetGirthLocal(float height, BlendshapeType type) {
            return girthCurves[(int)type].Evaluate(height);
        }
        public float GetGirthWorld(float height, BlendshapeType type) {
            if (dickTransform == null) {
                return 0f;
            }
            return GetGirthLocal(height, type) * dickTransform.lossyScale.x;
        }
        public Vector3 GetXOffsetLocal(float height, BlendshapeType type) {
            return xOffsetCurves[(int)type].Evaluate(height) * dickRightAxis;
        }
        public Vector3 GetYOffsetLocal(float height, BlendshapeType type) {
            return yOffsetCurves[(int)type].Evaluate(height) * dickUpAxis;
        }
        public Vector3 GetXYZOffsetWorld(float height, BlendshapeType type) {
            return dickTransform.TransformPoint(GetXOffsetLocal(height, type) + GetYOffsetLocal(height, type) + GetZOffsetLocal(height)) - dickTransform.position;
        }
        public Vector3 GetZOffsetLocal(float height) {
            return height * dickForwardAxis;
        }
        public float GetGirthLocal(float height, List<float> weights) {
            float baseGirth = girthCurves[(int)BlendshapeType.None].Evaluate(height);
            float girth = baseGirth;
            for (int i = 0; i < girthCurves.Count; i++) {
                girth += (girthCurves[i].Evaluate(height) - baseGirth) * weights[i];
            }
            return girth;
        }
        public float GetGirthWorld(float height, List<float> weights) {
            if (dickTransform == null) {
                return 0f;
            }
            return GetGirthLocal(height, weights) * dickTransform.lossyScale.x;
        }
        public float GetSlopeLocal(float height, List<float> weights) {
            float baseSlope = girthCurves[(int)BlendshapeType.None].Differentiate(height);
            for (int i = 0; i < girthCurves.Count; i++) {
                baseSlope += (girthCurves[i].Differentiate(height) - baseSlope) * weights[i];
            }
            //Debug.DrawLine(GetXYZOffsetWorld(height, weights) + dickTransform.position, GetXYZOffsetWorld(height, weights) + dickTransform.position + dickTransform.TransformDirection(dickUpAxis));
            return baseSlope * (1f - (height / GetLocalLength(weights)));
        }
        public Vector3 GetXOffsetLocal(float height, List<float> weights) {
            float baseXOffset = xOffsetCurves[(int)BlendshapeType.None].Evaluate(height);
            float xOffset = baseXOffset;
            for (int i = 0; i < xOffsetCurves.Count; i++) {
                xOffset += (xOffsetCurves[i].Evaluate(height) - baseXOffset) * weights[i];
            }
            if (xOffsetCurves.Count != weights.Count) {
                Debug.LogError(gameObject.name);
            }
            return xOffset * dickRightAxis;
        }
        public Vector3 GetYOffsetLocal(float height, List<float> weights) {
            float baseYOffset = yOffsetCurves[(int)BlendshapeType.None].Evaluate(height);
            float yOffset = baseYOffset;
            for (int i = 0; i < yOffsetCurves.Count; i++) {
                yOffset += (yOffsetCurves[i].Evaluate(height) - baseYOffset) * weights[i];
            }
            return yOffset * dickUpAxis;
        }
        public Vector3 GetXOffsetWorld(float height, List<float> weights) {
            return dickTransform.TransformPoint(GetXOffsetLocal(height, weights)) - dickTransform.position;
        }
        public Vector3 GetYOffsetWorld(float height, List<float> weights) {
            return dickTransform.TransformPoint(GetYOffsetLocal(height, weights)) - dickTransform.position;
        }
        public Vector3 GetXYOffsetWorld(float height, List<float> weights) {
            return dickTransform.TransformPoint(GetXOffsetLocal(height, weights) + GetYOffsetLocal(height, weights)) - dickTransform.position;
        }
        public Vector3 GetXYOffsetWorld(float height, BlendshapeType type) {
            return dickTransform.TransformPoint(GetXOffsetLocal(height, type) + GetYOffsetLocal(height, type)) - dickTransform.position;
        }
        public Vector3 GetXYZOffsetWorld(float height, List<float> weights) {
            return dickTransform.TransformPoint(GetXOffsetLocal(height, weights) + GetYOffsetLocal(height, weights) + GetZOffsetLocal(height)) - dickTransform.position;
        }
        public float ScatterSampleDerivative(float worldDistance, float sampleStepDistance, int samples = 4) {
            float height = GetHeightSample(worldDistance);
            float length = GetLocalLength(weights);
            float stepHeight = GetHeightSample(sampleStepDistance);
            if (height > length) {
                height = length - (float)(samples / 2) * stepHeight;
            }
            float avgSlope = 0f;
            for (int i = -samples / 2; i <= samples / 2; i++) {
                avgSlope += GetSlopeLocal(height + (float)i * stepHeight, weights);
            }
            avgSlope /= samples;
            return avgSlope;
        }
        public void Awake() {
            weights.Clear();
            foreach (BlendshapeType type in Enum.GetValues(typeof(BlendshapeType))) {
                weights.Add(0f);
            }
            lengths.Clear();
            foreach (BlendshapeType type in Enum.GetValues(typeof(BlendshapeType))) {
                int keyCount = girthCurves[(int)type].keys.Length;
                lengths.Add(girthCurves[(int)type].keys[keyCount - 1].time - girthCurves[(int)type].keys[0].time);
            }
            lastDistance = 0f;
            for (int i = 0; i < strandCount; i++) {
                strands.Add(null);
                GameObject strandSurfaceTransform = new GameObject("StrandPoint");
                float strandDist = ((float)i * ((float)GetLocalLength(weights) / (float)strandCount));
                strandSurfaceTransform.transform.position = GetRandomPointOnSurface(strandDist);
                strandSurfaceTransform.transform.parent = dickTransform;
                randomSurfacePoints.Add(strandSurfaceTransform.transform);
            }
            if (deformationTargets.Count > 0) {
                for (int i = 0; i < deformationTargets[0].sharedMesh.bindposes.Length; i++) {
                    if (deformationTargets[0].bones[i] == dickTransform) {
                        bindPoseDickTransformCache = deformationTargets[0].sharedMesh.bindposes[i];
                        break;
                    }
                }
            }
            slidingSoundSource = gameObject.AddComponent<AudioSource>();
            slidingSoundSource.clip = slidingSound;
            slidingSoundSource.spatialBlend = 1f;
            slidingSoundSource.rolloffMode = AudioRolloffMode.Logarithmic;
            slidingSoundSource.maxDistance = 10f;
            slidingSoundSource.minDistance = 0.1f;
            slidingSoundSource.loop = true;
            slidingSoundSource.Stop();
        }
        public float GetLocalLength(List<float> weights) {
            if (lengths.Count != girthCurves.Count) {
                lengths.Clear();
                foreach (BlendshapeType type in Enum.GetValues(typeof(BlendshapeType))) {
                    int keyCount = girthCurves[(int)type].keys.Length;
                    lengths.Add(girthCurves[(int)type].keys[keyCount - 1].time - girthCurves[(int)type].keys[0].time);
                }
            }
            float baseLength = lengths[(int)BlendshapeType.None];
            float length = baseLength;
            for (int i = 0; i < lengths.Count; i++) {
                length += (lengths[i] - baseLength) * weights[i];
            }
            return length;
        }
        public float GetWorldLength(List<float> weights) {
            return GetLocalLength(weights) * dickTransform.lossyScale.x;
        }
        public float GetMinPenetrationDepth() {
            if (minPen.Count != girthCurves.Count) {
                minPen.Clear();
                for (int i = 0; i < girthCurves.Count; i++) {
                    int keyCount = girthCurves[i].keys.Length;
                    minPen.Add(girthCurves[i].keys[0].time);
                }
            }
            return minPen.Min();
        }

        public float GetMaxLength() {
            if (lengths.Count != girthCurves.Count) {
                lengths.Clear();
                for (int i = 0; i < girthCurves.Count; i++) {
                    int keyCount = girthCurves[i].keys.Length;
                    lengths.Add(girthCurves[i].keys[keyCount - 1].time - girthCurves[i].keys[0].time);
                }
            }
            float max = float.MinValue;
            foreach (float length in lengths) {
                max = Mathf.Max(length, max);
            }
            return max;
        }
        public float GetWorldLength() {
            if (weights == null || weights.Count == 0) {
                weights = new List<float>();
                foreach (BlendshapeType type in Enum.GetValues(typeof(BlendshapeType))) {
                    weights.Add(0f);
                }
            }
            return GetWorldLength(weights);
        }
        public void Deform(Material m, float height, float cumHeight, float cumAmount, List<float> weights) {
            if (deformationTargets.Count == 0) {
                return;
            }
            if (holeTarget != null) {
                m.SetVector("_OrificePosition", holeTarget.fakeHoleGameObject.transform.position);
                m.SetVector("_OrificeNormal", holeTarget.fakeHoleGameObject.transform.TransformDirection(holeTarget.holeForwardAxis));
            } else {
                m.SetVector("_OrificePosition", Vector3.zero);
            }
            m.SetFloat("_SquishAmount", weights[(int)Dick.BlendshapeType.Squish]);
            m.SetFloat("_PullAmount", weights[(int)Dick.BlendshapeType.Pull]);
            m.SetFloat("_CumAmount", weights[(int)Dick.BlendshapeType.Cum]);
            m.SetFloat("_HoleProgress", height);
            m.SetFloat("_ModelScale", blendshapeSoftness * dickTransform.lossyScale.x);
            m.SetFloat("_BlendshapeMultiplier", dickTransform.lossyScale.x * bindPoseDickTransformCache.lossyScale.x);
            m.SetVector("_DickOrigin", bakeMeshes[0].rootBone.worldToLocalMatrix.MultiplyPoint(dickTransform.position) * bakeMeshes[0].rootBone.lossyScale.x);
            m.SetVector("_DickForward", Vector3.Normalize(bakeMeshes[0].rootBone.worldToLocalMatrix.MultiplyVector(dickTransform.TransformDirection(dickForwardAxis))));
            m.SetVector("_DickRight", Vector3.Normalize(bakeMeshes[0].rootBone.worldToLocalMatrix.MultiplyVector(dickTransform.TransformDirection(dickRightAxis))));
            m.SetVector("_DickUp", Vector3.Normalize(bakeMeshes[0].rootBone.worldToLocalMatrix.MultiplyVector(dickTransform.TransformDirection(dickUpAxis))));
            m.SetFloat("_DickLength", GetLocalLength(weights) * dickTransform.lossyScale.x);
            m.SetFloat("_CumProgress", cumHeight);
            m.SetFloat("_CumAmount", cumAmount);
        }
        public void SetDeformations(float height, float cumHeight, float cumAmount, List<float> weights) {
            if (materials.Count == 0) {
                Start();
            }
            foreach (Material r in materials) {
                Deform(r, height, cumHeight, cumAmount, weights);
            }
        }
        public void LateUpdate() {
            for (int i = 0; i < strands.Count; i++) {
                if (strands[i] != null && holeTarget != null) {
                    strands[i].targetOffsetMultiplier = holeTarget.realGirth / 2f;
                }
            }
            pushPullAmount -= pushPullAmount * Time.deltaTime * 0.25f;
            if (holeTarget == null || holeTarget.fakeHoleGameObject == null) {
                float ch = cumProgress * GetLocalLength(weights);
                SetDeformations(10000000, ch * dickTransform.lossyScale.x, cumActive, weights);
                //aim.constraintActive = false;
                hitBoxCollider.localScale = Vector3.one - dickForwardAxis + dickForwardAxis * 1f;
                slidingSoundSource.Stop();
                return;
            }
            //} else {
            //if (aim.sourceCount <= 0) {
            //ConstraintSource cs = new ConstraintSource();
            //cs.sourceTransform = holeTarget.fakeHoleGameObject.transform;
            //cs.weight = 1f;
            //aim.AddSource(cs);
            //aim.constraintActive = true;
            //} else {
            //ConstraintSource cs = new ConstraintSource();
            //cs.sourceTransform = holeTarget.fakeHoleGameObject.transform;
            //cs.weight = 1f;
            //aim.SetSource(0, cs);
            //aim.constraintActive = true;
            //}
            //}
            float distance = Vector3.Distance(dickTransform.position, holeTarget.GetSamplePosition());
            float unalteredDistance = Vector3.Distance(dickTransform.position, holeTarget.GetUnalteredSamplePosition());
            if (Vector3.Dot(holeTarget.GetSamplePosition() - dickTransform.position, dickTransform.TransformDirection(dickForwardAxis)) < 0f) {
                unalteredDistance = 0f;
                distance = 0f;
            }
            int neededStrands = Mathf.FloorToInt(Mathf.Max(GetWorldLength() - distance, 0f) / GetWorldLength() * (float)strandCount);
            for (int i = strands.Count - 1; i >= Mathf.Max(strands.Count - neededStrands, 0); i--) {
                if (strands[i] == null) {
                    Strand strand = randomSurfacePoints[i].gameObject.AddComponent<Strand>();
                    strand.target = holeTarget.transform;
                    float strandDist = ((float)i * ((float)GetLocalLength(weights) / (float)strandCount));
                    randomSurfacePoints[i].position = GetRandomPointOnSurface(strandDist);
                    strand.maxDistance *= UnityEngine.Random.Range(0.5f, 1.5f);
                    strand.targetOffset = Vector3.ProjectOnPlane(randomSurfacePoints[i].localPosition, Vector3.up).normalized;
                    strand.volume *= UnityEngine.Random.Range(0.25f, 1f);
                    strand.lineMaterial = strandMaterial;
                    strands[i] = strand;
                }
            }
            for (int i = 0; i < strands.Count; i++) {
                if (strands[i] != null) {
                    float strandDist = ((float)i * ((float)GetWorldLength() / (float)strandCount));
                    strands[i].SetAlpha(Mathf.Clamp01((distance - strandDist) * 10f));
                }
            }
            float length = GetWorldLength();
            aimWeight = 0f;
            if (distance > length) {
                if (penetrating) {
                    penetrating = false;
                    OnDepenetrate.Invoke();
                }
                slidingSoundSource.Stop();
                aimWeight = Mathf.Clamp01(1f - ((distance - (length)) * 25f));
            } else {
                if (!penetrating) {
                    penetrating = true;
                    OnPenetrate.Invoke();
                    kobold?.PumpUpDick(1f);
                    if (stream != null) {
                        stream.StopFiring();
                    }
                    slidingSoundSource.Play();
                }
                aimWeight = 1f;
                kobold?.PumpUpDick(1f);
                PenetrateContinuous.Invoke();
            }
            float distanceDiff = unalteredDistance - lastDistance;
            lastDistance = unalteredDistance;
            float height = GetHeightSample(distance);
            float hitboxSize = Mathf.Clamp01(Mathf.Max(distance / Mathf.Max(GetWorldLength(), 0.1f), 0.1f));

            hitBoxCollider.localScale = Vector3.one - dickForwardAxis + dickForwardAxis * Mathf.Clamp(hitboxSize, 0.25f, 1f);
            pushPullLerper = Mathf.Lerp(pushPullLerper, pushPullAmount, Time.deltaTime * 5f);
            weights[(int)BlendshapeType.Squish] = Mathf.Clamp01(pushPullLerper);
            weights[(int)BlendshapeType.Pull] = Mathf.Clamp01(-pushPullLerper);
            float cumHeight = cumProgress * GetMaxLength();
            SetDeformations(height * dickTransform.lossyScale.x, cumHeight * dickTransform.lossyScale.x, cumActive, weights);
            float sizeScaler = 10 / (blendshapeSoftness);
            float sharpnessCalc = (blendshapeSoftness) * 0.05f;
            float cumScaler = 1f - Mathf.Clamp01((Mathf.Abs(height - cumHeight) - sharpnessCalc) * sizeScaler);
            weights[(int)BlendshapeType.Cum] = cumScaler * cumActive;
            float girth = GetGirthWorld(height, weights);
            float adj = 1f - Mathf.Clamp01(height - (GetMaxLength() * 1.1f)) * 5f;
            if (distanceDiff > 0 && height > GetLocalLength(weights) * 0.8f) {
                adj = 0f;
            }

            if (height < GetMaxLength() * 0.4f && !playedPlap) {
                playedPlap = true;
                GameManager.instance.SpawnAudioClipInWorld(plappingSounds[UnityEngine.Random.Range(0, plappingSounds.Count)], transform.position, Mathf.Clamp01(transform.lossyScale.x * 0.5f));
            } else if (height > GetMaxLength() * 0.5f) {
                playedPlap = false;
            }

            holeTarget.AddSlideForce(-distanceDiff * 0.25f * adj);
            AddSlideForce(-distanceDiff * 0.25f * adj);
            slidingSoundSource.volume = Mathf.Clamp01(Mathf.Abs(distanceDiff * 10f));

            //Vector3 offset = GetXYOffsetWorld(height, weights);
            holeTarget.SetGirth(girth);
        }
        public float GetMaxVolume() {
            return float.MaxValue;
        }
        private bool grabbed = false;
        private HashSet<Collider> IgnoringCollisions = new HashSet<Collider>();
        public List<Collider> selfColliders = new List<Collider>();
        private Kobold internalKobold;
        private bool waiting = false;
        /*public void OnGrab() {
            grabbed = true;
        }
        public void OnRelease() {
            foreach(Collider c in IgnoringCollisions) {
                foreach (Collider d in selfColliders) {
                    Physics.IgnoreCollision(c, d, false);
                }
            }
            IgnoringCollisions.Clear();
            if (dick.holeTarget != null) {
                dick.holeTarget.dickTarget = null;
            }
            dick.holeTarget = null;
            grabbed = false;
        }*/
        //public IEnumerable WaitAndThenStopPenetrating() {
        //yield return new WaitForSeconds(1f);
        //waiting = false;
        //}
        public void FixedUpdate() {
            if (holeTarget == null) {
                return;
            }
            if (holeTarget == null
                || holeTarget.dickTarget != this
                || (holeTarget.aimWeight == 0)
                //|| Vector3.Distance(dick.dickTransform.position, dick.holeTarget.GetComponent<Naelstrof.Penetratable>().fakeHoleGameObject.transform.position) > dick.GetMaxLength()*dick.dickTransform.lossyScale.x*1.1f 
                || !isActiveAndEnabled
                ) {
                // Give it a second, just in case we're getting thrusted right back in.
                holeTarget.dickTarget = null;
                holeTarget = null;
                //dick.body.maxAngularVelocity = 7f;
                UnignoreAll();
            }
            //  Kobold k = dick.holeTarget.GetComponentInParent<Kobold>();
            // IgnoreCollision(k);
            //}
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
                    IgnoringCollisions.Add(e);
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
                foreach (Collider c in IgnoringCollisions) {
                    if (c == null) {
                        continue;
                    }
                    Physics.IgnoreCollision(c, d, false);
                }
            }
            IgnoringCollisions.Clear();
        }
        public void CheckCollision(Collider collider) {
            if (!isActiveAndEnabled || arousal < 0.8f) {
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
            if (k.activeDicks.Count > 0 && k.activeDicks[0].dick.holeTarget != null && kobold == k.activeDicks[0].dick.holeTarget.transform.root.GetComponent<Kobold>()) {
                return;
            }
            // Don't penetrate a kobold that's already penetrating us
            if (k.activeDicks.Count > 0 && k.activeDicks[0].dick.holeTarget != null && k.activeDicks[0].dick.holeTarget.transform.root.GetComponent<Kobold>() == kobold) {
                return;
            }
            if (holeTarget != null) {
                return;
            }
            Naelstrof.Penetratable closestPenetratable = collider.GetComponent<Naelstrof.Penetratable>();
            if (closestPenetratable != null) {
                float dist = Vector3.Distance(transform.position, closestPenetratable.transform.position);
                float angleDiff = Vector3.Dot(-closestPenetratable.fakeHoleGameObject.transform.TransformDirection(closestPenetratable.holeForwardAxis), dickTransform.TransformDirection(dickForwardAxis));
                if (closestPenetratable.dickTarget == null && angleDiff > -0.25f) {
                    if (dist > GetWorldLength()) {
                        return;
                    }
                    IgnoreCollision(k);
                    //dick.body.maxAngularVelocity = 64f;
                    holeTarget = closestPenetratable;
                    closestPenetratable.aimWeight = 0.0001f;
                    holeTarget.dickTarget = this;
                }
            }
        }
        public void OnTriggerEnter(Collider collider) {
            CheckCollision(collider);
        }
        public void OnTriggerStay(Collider collider) {
            CheckCollision(collider);
        }
    }
}
