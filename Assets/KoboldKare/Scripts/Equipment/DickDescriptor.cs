using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using PenetrationTech;
using KoboldKare;
using Naelstrof.Easing;
using Naelstrof.Inflatable;
using Naelstrof.Mozzarella;
using NetStack.Serialization;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(DickDescriptor))]
public class DickInfoEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }
    public void OnSceneGUI(){
        DickDescriptor t = (DickDescriptor)target;
        int i = 0;
        foreach(DickDescriptor.DickSet set in t.dicks) {
            if (set.dick == null) {
                continue;
            }
            Vector3 globalPosition = Handles.PositionHandle(t.transform.TransformPoint(set.attachPosition), t.transform.rotation);
            if (Vector3.Distance(t.transform.InverseTransformPoint(globalPosition), set.attachPosition) > 0.01f) {
                set.attachPosition = t.transform.InverseTransformPoint(globalPosition);
                EditorUtility.SetDirty(target);
            }
            Handles.Label(t.transform.TransformPoint(set.attachPosition), "DICK " + i++ + " ATTACH");
        }
    }
}

#endif
// DickInfo is mainly used to have an in-scene reference to a bunch of dick info. Most of the functionality of a dick is split between DickEquipment.cs, and an external addon called PenetrationTech
public class DickDescriptor : MonoBehaviour {
    public delegate void CumThroughAction(Penetrable penetrable);

    public static event CumThroughAction cumThrough;
    private static readonly int BrightnessContrastSaturation = Shader.PropertyToID("_HueBrightnessContrastSaturation");
    private Kobold attachedKobold;
    private bool cumming = false;
    [HideInInspector]
    public int equipmentInstanceID;

    //[PenetratorListener(typeof(KoboldDickListener), "Kobold Dick Listener")]
    private class KoboldDickListener : PenetratorListener {
        public KoboldDickListener(Kobold kobold, DickSet set) {
            attachedKobold = kobold;
            dickSet = set;
        }

        private readonly Kobold attachedKobold;
        private DickSet dickSet;
        private float lastDepthDist;
        private Penetrable penetrableMem;
        public override void OnPenetrationStart(Penetrable penetrable) {
            base.OnPenetrationStart(penetrable);
            penetrableMem = penetrable;
        }

        protected override void OnPenetrationDepthChange(float depthDist) {
            base.OnPenetrationDepthChange(depthDist);
            float movementAmount = depthDist - lastDepthDist;
            attachedKobold.PumpUpDick(Mathf.Abs(movementAmount*0.15f));
            attachedKobold.AddStimulation(Mathf.Abs(movementAmount));
            lastDepthDist = depthDist;
            ClipListener clipListener = (ClipListener)penetrableMem.listeners.Find((o) => o is ClipListener);
            dickSet.inside = depthDist != 0f && (depthDist < penetrableMem.GetPath().arcLength || (clipListener != null && !clipListener.GetAllowForAllTheWayThrough()));
            dickSet.overpenetrated = depthDist >= penetrableMem.GetPath().arcLength;
        }
    }

    [System.Serializable]
    public class DickSet {
        public Transform dickContainer;
        public PenetrationTech.Penetrator dick;
        
        public Inflatable ballSizeInflater;
        public Inflatable dickSizeInflater;
        public Inflatable bonerInflater;
        
        public Equipment.AttachPoint attachPoint;
        public Material cumSplatProjectorMaterial;

        public Vector3 attachPosition;
        public AudioPack cumSoundPack;

        [FormerlySerializedAs("info")] [HideInInspector]
        public DickDescriptor descriptor;

        public bool shouldDisableVagina = true;

        public void Destroy() {
            GameObject.Destroy(dick.gameObject);
        }

        public bool inside { get; set; }
        public bool overpenetrated { get; set; }
    }
    public List<DickSet> dicks = new List<DickSet>();
    public void Initialize() {
        foreach (DickSet set in dicks) {
            //set.ball
            set.descriptor = this;
            set.bonerInflater.OnEnable();
            set.dickSizeInflater.OnEnable();
            set.ballSizeInflater.OnEnable();
        }
    }
    public void RemoveFrom(Kobold k) {
        // Must've been removed already
        foreach (DickSet set in dicks) {
            if (!k.activeDicks.Contains(set)) {
                return;
            }
        }

        bool shouldReenableVagina = false;
        foreach (DickSet set in dicks) {
            k.activeDicks.Remove(set);
            foreach (var penset in k.penetratables) {
                //if (penset.penetratable.name.Contains("Mouth")) {
                    //continue;
                //}
                set.dick.RemoveIgnorePenetrable(penset.penetratable);
            }
            shouldReenableVagina |= set.shouldDisableVagina;
        }
        if (shouldReenableVagina) {
            foreach(var hole in k.penetratables) {
                if (hole.isFemaleExclusiveAnatomy) {
                    hole.penetratable.gameObject.SetActive(true);
                }
            }
        }
        if (k == attachedKobold) {
            attachedKobold = null;
        }
    }
    public IEnumerator CumRoutine(DickSet set) {
        if (cumming) {
            yield return new WaitUntil(() => !cumming);
        }

        cumming = true;
        float ballSize = attachedKobold.GetGenes().ballSize;
        // (1-1/(x/maxInput+1)) * maxPossibleResult
        float pulsesSample = (1f - 1f / (ballSize / 100f + 1f)) * 60f + 5f;
        int pulses = Mathf.CeilToInt(pulsesSample);
        float pulseDuration = 0.8f;
        for (int i = 0; i < pulses; i++) {
            GameManager.instance.SpawnAudioClipInWorld(set.cumSoundPack, set.dick.transform.position);
            float pulseStartTime = Time.time;
            while (Time.time < pulseStartTime+pulseDuration) {
                float t = ((Time.time - pulseStartTime) / pulseDuration);
                foreach (var renderTarget in set.dick.GetTargetRenderers()) {
                    Mesh mesh = ((SkinnedMeshRenderer)renderTarget.renderer).sharedMesh;
                    List<string> cumBlendShapeIndices = new List<string>();
                    for (int j = 0; j < mesh.blendShapeCount; j++) {
                        string blendShapeName = mesh.GetBlendShapeName(j);
                        if (blendShapeName.ToLower(CultureInfo.InvariantCulture).StartsWith("cum")) {
                            cumBlendShapeIndices.Add(blendShapeName);
                        }
                    }
                    cumBlendShapeIndices.Sort();
                    
                    float stepSize = 1f / (cumBlendShapeIndices.Count + 1);
                    for (int j = 0; j < cumBlendShapeIndices.Count; j++) {
                        float easingStage = Mathf.Clamp01(Easing.Cubic.InOut(1f-(Mathf.Abs(t-stepSize*(j+1))*4f)));
                        ((SkinnedMeshRenderer)renderTarget.renderer).SetBlendShapeWeight(mesh.GetBlendShapeIndex(cumBlendShapeIndices[j]), easingStage*100f);
                    }
                }
                yield return null;
            }
            foreach (var renderTarget in set.dick.GetTargetRenderers()) {
                Mesh mesh = ((SkinnedMeshRenderer)renderTarget.renderer).sharedMesh;
                for (int j = 0; j < mesh.blendShapeCount; j++) {
                    string blendShapeName = mesh.GetBlendShapeName(j);
                    if (blendShapeName.ToLower(CultureInfo.InvariantCulture).StartsWith("cum")) {
                        ((SkinnedMeshRenderer)renderTarget.renderer).SetBlendShapeWeight(mesh.GetBlendShapeIndex(blendShapeName), 0f);
                    }
                }
            }

            if (!set.dick.TryGetPenetrable(out Penetrable pennedHole) || !set.inside || pennedHole.GetComponentInParent<GenericReagentContainer>() == null) {
                if (set.overpenetrated) {
                    cumThrough?.Invoke(pennedHole);
                }

                if (MozzarellaPool.instance.TryInstantiate(out Mozzarella mozzarella)) {
                    ReagentContents alloc = new ReagentContents();
                    alloc.AddMix(ReagentDatabase.GetReagent("Cum").GetReagent(attachedKobold.GetGenes().ballSize/pulses));
                    mozzarella.SetVolumeMultiplier(alloc.volume*2f);
                    Color color = alloc.GetColor();
                    mozzarella.SetLineColor(color);
                    mozzarella.hitCallback += (hit, startPos, dir, length, volume) => {
                        if (attachedKobold == null) {
                            return;
                        }
                        if (attachedKobold.photonView.IsMine) {
                            GenericReagentContainer container =
                                hit.collider.GetComponentInParent<GenericReagentContainer>();
                            if (container != null && attachedKobold != null) {
                                BitBuffer buffer = new BitBuffer(4);
                                buffer.AddReagentContents(alloc.Spill(alloc.volume * 0.1f));
                                container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All,
                                    buffer, attachedKobold.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
                            }
                        }

                        //Debug.DrawLine(hit.point, hit.point + hit.normal, Color.red, 5f);
                        if (alloc.volume > 0f) {
                            set.cumSplatProjectorMaterial.color = color;
                        }
                        PaintDecal.RenderDecalForCollider(hit.collider, set.cumSplatProjectorMaterial,
                            hit.point - hit.normal * 0.1f, Quaternion.LookRotation(hit.normal, Vector3.up)*Quaternion.AngleAxis(UnityEngine.Random.Range(-180f,180f), Vector3.forward),
                            Vector2.one * (volume * 4f), length);
                    };
                    mozzarella.SetFollowPenetrator(set.dick);
                }
                continue;
            }
            Vector3 holePos = pennedHole.GetPath().GetPositionFromT(0f);
            Vector3 holeTangent = pennedHole.GetPath().GetVelocityFromT(0f);
            PaintDecal.RenderDecalInSphere(holePos, set.dick.transform.lossyScale.x * 0.25f,
                set.cumSplatProjectorMaterial, Quaternion.LookRotation(holeTangent, Vector3.up),
                GameManager.instance.decalHitMask);
            GenericReagentContainer container = pennedHole.GetComponentInParent<GenericReagentContainer>();
            if (attachedKobold.photonView.IsMine) {
                ReagentContents alloc = new ReagentContents();
                alloc.AddMix(ReagentDatabase.GetReagent("Cum").GetReagent(attachedKobold.GetGenes().ballSize/pulses));
                BitBuffer reagentBuffer = new BitBuffer(4);
                reagentBuffer.AddReagentContents(alloc);
                container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, reagentBuffer,
                    attachedKobold.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
            }
        }
        cumming = false;
    }
    public void AttachTo(Kobold k) {
        Initialize();
        // Kobolds are kinda loaded asyncronously, so awake most certainly needs to be called.
        attachedKobold = k;
        var attachedAnimator = k.GetComponent<CharacterDescriptor>().GetDisplayAnimator();
        bool animatorWasEnabled = attachedAnimator.enabled;
        attachedAnimator.enabled = true;
        foreach(DickSet set in dicks) {
            Vector3 scale = set.dickContainer.localScale;
            set.descriptor = this;
            set.dickContainer.parent = k.GetAttachPointTransform(set.attachPoint);
            set.dickContainer.localScale = scale;
            set.dickContainer.transform.localPosition = -set.attachPosition;
            set.dickContainer.transform.localRotation = Quaternion.identity;
            foreach (var penset in k.penetratables) {
                set.dick.AddIgnorePenetrable(penset.penetratable);
            }

            if (set.shouldDisableVagina) {
                foreach(var hole in attachedKobold.penetratables) {
                    if (hole.isFemaleExclusiveAnatomy) {
                        hole.penetratable.gameObject.SetActive(false);
                    }
                }
            }

            set.dick.listeners.Add(new KoboldDickListener(k,set));
            k.activeDicks.Add(set);
        }
        foreach(DickSet set in dicks) {
            foreach(JigglePhysics.JiggleRigBuilder rig in set.dick.GetComponentsInChildren<JigglePhysics.JiggleRigBuilder>()) {
                rig.enabled = true;
            }
        }
        attachedAnimator.enabled = animatorWasEnabled;
    }

    //private void OnDestroy() {
        //RemoveFrom(attachedKobold);
    //}
}
