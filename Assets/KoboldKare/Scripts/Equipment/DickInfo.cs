using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PenetrationTech;
using KoboldKare;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(DickInfo))]
public class DickInfoEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
    }
    public void OnSceneGUI(){
        DickInfo t = (DickInfo)target;
        int i = 0;
        foreach(DickInfo.DickSet set in t.dicks) {
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
// DickInfo is mainly used to have an in-scene reference to a bunch of dick info. Most of the functionality of a dick is split between DickEquipment.cs, and Dick.cs
public class DickInfo : MonoBehaviour {
    private Task attachTask;
    private Kobold attachedKobold;
    [System.Serializable]
    public class DickSet {
        public Transform dickContainer;
        public PenetrationTech.Penetrator dick;
        public GenericInflatable dickInflater;
        public GenericInflatable bonerInflator;
        public GenericInflatable balls;
        public Equipment.AttachPoint attachPoint;

        public Vector3 attachPosition;

        [HideInInspector]
        public DickInfo info;
        public HumanBodyBones parent;
        [HideInInspector]
        public Transform parentTransform;
        [HideInInspector]
        public int dickIdentifier;
        public void Destroy() {
            GameObject.Destroy(dick.gameObject);
        }
    }
    public List<DickSet> dicks = new List<DickSet>();
    public void Awake() {
        foreach (DickSet set in dicks) {
            set.info = this;
        }
    }
    public void AttachTo(Kobold k) {
        if (attachTask != null){
            attachTask.Stop();
        }
        attachTask = new Task(AttachToRoutine(k));
    }
    public void OnDickLODClose() {
        foreach (DickSet set in dicks) {
            if (set.dick.body != null && !set.dick.body.isKinematic) {
                set.dick.body.interpolation = RigidbodyInterpolation.Interpolate;
                set.dick.body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }
    }
    public void OnDickLODFar() {
        foreach (DickSet set in dicks) {
            if (set.dick.body != null && !set.dick.body.isKinematic) {
                set.dick.body.interpolation = RigidbodyInterpolation.None;
                set.dick.body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }
    }
    public void OnDickMovement(float movementAmount) {
        attachedKobold.PumpUpDick(Mathf.Abs(movementAmount));
        attachedKobold.AddStimulation(Mathf.Abs(movementAmount));
    }
    //public void Cum() {
        //foreach (DickSet set in dicks) {
            //set.dick.Cum();
        //}
    //}
    public void RemoveFrom(Kobold k) {
        foreach (DickSet set in dicks) {
            foreach(var r in set.dick.deformationTargets) {
                k.koboldBodyRenderers.Remove(r);
            }
            set.balls.SetContainer(null);
            k.activeDicks.Remove(set);
            set.dick.OnMove.RemoveListener(OnDickMovement);
            foreach(Rigidbody r in k.ragdollBodies) {
                if (r.GetComponent<Collider>() == null) {
                    continue;
                }
                //foreach (Collider b in set.dick.selfColliders) {
                    //Physics.IgnoreCollision(r.GetComponent<Collider>(), b, false);
                //}
            }
        }
        bool shouldReenableVagina = true;
        foreach(var dick in k.activeDicks) {
            if (dick.parent == HumanBodyBones.Hips) {
                shouldReenableVagina = false;
            }
        }
        if (shouldReenableVagina) {
            foreach(var hole in k.penetratables) {
                if (hole.isFemaleExclusiveAnatomy) {
                    hole.penetratable.gameObject.SetActive(true);
                }
            }
        }
        //k.OnOrgasm.RemoveListener(Cum);
        k.lodLevel.OnLODClose.RemoveListener(OnDickLODClose);
        k.lodLevel.OnLODFar.RemoveListener(OnDickLODFar);
        if (attachedKobold) {
            attachedKobold.RagdollEvent -= OnRagdoll;
        }
        if (k == attachedKobold) {
            attachedKobold = null;
        }
        //Destroy(gameObject);
    }
    private void OnDestroy() {
        if (attachedKobold) {
            attachedKobold.RagdollEvent -= OnRagdoll;
        }
    }
    private IEnumerator AttachToRoutine(Kobold k) {
        attachedKobold = k;
        // We need to make sure that our model isn't disabled, otherwise k.animator.GetBoneTransform always returns null :weary:
        while(!k.gameObject.activeInHierarchy) {
            yield return new WaitUntil(()=>k.gameObject.activeInHierarchy);
        }
        // Kobold, or the dicks must've been destroyed before we got to attach. Abort!
        if (k == null || dicks == null || dicks.Count <= 0 || dicks[0] == null || dicks[0].dickContainer == null) {
            yield break;
        }
        foreach(DickSet set in dicks) {
            foreach(JigglePhysics.JiggleBone bone in set.dick.GetComponentsInChildren<JigglePhysics.JiggleBone>(true)) {
                bone.enabled = false;
            }
            foreach(Rigidbody b in set.dick.GetComponentsInChildren<Rigidbody>(true)) {
                b.isKinematic = true;
            }
        }
        bool animatorWasEnabled = k.animator.enabled;
        k.animator.enabled = true;
        foreach(DickSet set in dicks) {
            Vector3 scale = set.dickContainer.localScale;
            set.parentTransform = k.animator.GetBoneTransform(set.parent);
            while(set.parentTransform == null) {
                yield return new WaitUntil(()=>k.animator.isActiveAndEnabled);
                set.parentTransform = k.animator.GetBoneTransform(set.parent);
            }
            set.info = this;
            set.dick.root = k.transform;
            set.dickContainer.parent = k.attachPoints[(int)set.attachPoint];
            set.dickContainer.localScale = scale;
            set.dickContainer.transform.localPosition = -set.attachPosition;
            set.dickContainer.transform.localRotation = Quaternion.identity;

            set.dick.OnCumEmit.AddListener(()=>{
                //ReagentContents cumbucket = new ReagentContents();
                //cumbucket.Mix(set.balls.container.contents.Spill(set.balls.container.maxVolume/set.dick.cumPulseCount));
                //cumbucket.Mix(ReagentData.ID.Cum, set.dick.dickRoot.transform.lossyScale.x);
                Kobold pennedKobold = null;
                if (set.dick.holeTarget != null) {
                    pennedKobold = set.dick.holeTarget.GetComponentInParent<Kobold>();
                }
                // Add a little precum per-pulse.
                set.balls.GetContainer().AddMix(ReagentDatabase.GetReagent("Cum"), 1f+0.01f*attachedKobold.sizeInflatable.GetContainer().GetVolumeOf(ReagentDatabase.GetReagent("GrowthSerum"))+1f*attachedKobold.baseBallSize+1f*attachedKobold.baseDickSize, GenericReagentContainer.InjectType.Inject);
                if (!set.dick.IsInside() || pennedKobold == null) {
                    set.dick.GetComponentInChildren<FluidOutput>(true).Fire(set.balls.GetContainer());
                } else {
                    set.dick.holeTarget.GetComponentInParent<Kobold>().bellies[0].GetContainer().TransferMix(set.balls.GetContainer(), set.balls.GetContainer().maxVolume/set.dick.cumPulseCount, GenericReagentContainer.InjectType.Inject);
                }
            });

            if (set.parent == HumanBodyBones.Hips) {
                foreach(var hole in attachedKobold.penetratables) {
                    if (hole.isFemaleExclusiveAnatomy) {
                        hole.penetratable.gameObject.SetActive(false);
                    }
                }
            }
            k.koboldBodyRenderers.AddRange(set.dick.deformationTargets);
            set.balls.SetContainer(k.balls);
            k.activeDicks.Add(set);
            set.dick.OnMove.AddListener(OnDickMovement);
            // Make sure the dick is the right color, this just forces a reset of the colors.
            Color colorSave = k.HueBrightnessContrastSaturation;
            k.HueBrightnessContrastSaturation = Color.white;
            k.HueBrightnessContrastSaturation = colorSave;
        }
        foreach(DickSet set in dicks) {
            foreach(JigglePhysics.JiggleBone bone in set.dick.GetComponentsInChildren<JigglePhysics.JiggleBone>()) {
                bone.enabled = true;
            }
        }
        k.animator.enabled = animatorWasEnabled;
        k.lodLevel.OnLODClose.AddListener(OnDickLODClose);
        k.lodLevel.OnLODFar.AddListener(OnDickLODFar);
        if (attachedKobold) {
            attachedKobold.RagdollEvent += OnRagdoll;
        }
        OnRagdoll(attachedKobold.ragdolled);
    }
    public void OnRagdoll(bool ragdolled) {
        foreach (DickSet set in dicks) {
            foreach(Collider c in set.dick.body.GetComponentsInChildren<Collider>()) {
                if (c.isTrigger) {
                    continue;
                }
                c.enabled = !ragdolled;
            }
        }
    }
}
