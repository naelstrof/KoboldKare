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
        public GenericReagentContainer container;
        public Joint joint;

        public GenericInflatable balls;
        public Equipment.AttachPoint attachPoint;

        public Vector3 attachPosition;

        //[Tooltip("The body that the dick would be connected to if the character was ragdolled.")]
        //public Rigidbody ragdollAttachBody;
        [HideInInspector]
        public DickInfo info;
        public HumanBodyBones parent;
        [HideInInspector]
        public Transform parentTransform;
        [HideInInspector]
        public int dickIdentifier;
        [HideInInspector]
        public Vector3 dickAttachPosition;
        [HideInInspector]
        public Vector3 initialDickForwardHipSpace;
        [HideInInspector]
        public Vector3 initialDickUpHipSpace;
        [HideInInspector]
        public Quaternion initialBodyLocalRotation;
        [HideInInspector]
        public Vector3 initialBodyLocalPosition;
        [HideInInspector]
        public ConfigurableJoint rotJoint;
        [HideInInspector]
        public Quaternion initialTransformLocalRotation;
        [HideInInspector]
        public IJointData savedJoint;
        /*public void SetActive(bool active) {
            foreach (SkinnedMeshRenderer m in dick.deformationTargets) {
                if (m != null && m.gameObject != null) {
                    m.gameObject.SetActive(active);
                }
            }
            phaser.enabled = active;
            if (active) {
                phaser.dick = dick;
            }
            dick.body.gameObject.SetActive(active);
            dick.body.transform.position = parent.TransformPoint(dickAttachPosition);
            dick.gameObject.SetActive(active);
        }*/
        public void Destroy() {
            GameObject.Destroy(dick.gameObject);
        }
    }
    public void OnDestroy() {
        if (attachedKobold) {
            attachedKobold.RagdollEvent -= RagdollEvent;
        }
    }
    public List<DickSet> dicks = new List<DickSet>();
    public void Awake() {
        foreach (DickSet set in dicks) {
            // Create a joint default
            if (set.joint is ConfigurableJoint) {
                throw new UnityException("Configurable joints will cause problems! They won't get removed properly due to a unity bug, and using a while loop to remove them will sometimes delete freezes. So just use a CharacterJoint please!");
                //set.savedJoint = new ConfigurableJointData((ConfigurableJoint)set.joint);
            } else if (set.joint is CharacterJoint) {
                set.joint.autoConfigureConnectedAnchor = false;
                set.savedJoint = new CharacterJointData((CharacterJoint)set.joint);
            }
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
            if (set.balls != null) {
                k.balls.Remove(set.balls.container);
            }
            Destroy(set.rotJoint);
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
            var observables = set.dickContainer.GetComponentsInChildren<IPunObservable>(true);
            for (int i = 0; i < observables.Length; i++) {
                k.photonView.ObservedComponents.Remove((Component)observables[i]);
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
        k.RagdollEvent -= RagdollEvent;
        k.lodLevel.OnLODClose.RemoveListener(OnDickLODClose);
        k.lodLevel.OnLODFar.RemoveListener(OnDickLODFar);
        if (k == attachedKobold) {
            attachedKobold = null;
        }
        //Destroy(gameObject);
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
        }
        Quaternion oldRot = k.transform.rotation;
        k.transform.rotation = Quaternion.identity;
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
                ReagentContents cumbucket = new ReagentContents();
                cumbucket.Mix(set.balls.container.contents.Spill(set.balls.container.maxVolume/set.dick.cumPulseCount));
                cumbucket.Mix(ReagentData.ID.Cum, set.dick.dickRoot.transform.lossyScale.x);
                Kobold pennedKobold = null;
                if (set.dick.holeTarget != null) {
                    pennedKobold = set.dick.holeTarget.GetComponentInParent<Kobold>();
                }
                if (!set.dick.IsInside() || pennedKobold == null) {
                    set.dick.GetComponentInChildren<FluidOutput>(true).Fire(cumbucket, 2f);
                } else {
                    set.dick.holeTarget.GetComponentInParent<Kobold>().bellies[0].container.contents.Mix(cumbucket);
                }
            });

            set.initialBodyLocalRotation = set.dick.body.transform.localRotation;
            Rigidbody targetRigid = set.parentTransform.GetComponentInParent<Rigidbody>();
            Quaternion otherSavedRotation = targetRigid.transform.rotation;
            targetRigid.transform.rotation = Quaternion.identity;

            Physics.SyncTransforms();

            set.rotJoint = set.dick.body.gameObject.AddComponent<ConfigurableJoint>();
            set.rotJoint.connectedBody = targetRigid;
            set.rotJoint.anchor = Vector3.zero;
            set.rotJoint.connectedAnchor = Vector3.zero;

            var slerpd = set.rotJoint.slerpDrive;
            slerpd.positionSpring = 1000f;
            set.rotJoint.slerpDrive = slerpd;
            set.rotJoint.rotationDriveMode = RotationDriveMode.Slerp;
            set.rotJoint.targetRotation = Quaternion.identity;

            var observables = set.dickContainer.GetComponentsInChildren<IPunObservable>(true);
            for (int i = 0; i < observables.Length; i++) {
                k.photonView.ObservedComponents.Add((Component)observables[i]);
            }

            if (set.parent == HumanBodyBones.Hips) {
                foreach(var hole in attachedKobold.penetratables) {
                    if (hole.isFemaleExclusiveAnatomy) {
                        hole.penetratable.gameObject.SetActive(false);
                    }
                }
            }
            set.initialDickForwardHipSpace = set.parentTransform.InverseTransformDirection(set.dick.body.transform.forward);
            set.initialDickUpHipSpace = set.parentTransform.InverseTransformDirection(set.dick.body.transform.up);
            set.initialBodyLocalPosition = set.dick.body.transform.localPosition;
            set.dickAttachPosition = set.parentTransform.InverseTransformPoint(set.dick.dickRoot.position);
            set.initialTransformLocalRotation = set.dick.dickRoot.localRotation;
            set.joint.autoConfigureConnectedAnchor = false;
            set.joint.connectedBody = k.body;
            set.joint.massScale = 100f;
            set.joint.connectedAnchor = set.joint.connectedBody.transform.InverseTransformPoint(set.parentTransform.TransformPoint(set.dickAttachPosition));
            ((CharacterJointData)set.savedJoint).connectedBody = k.body;
            ((CharacterJointData)set.savedJoint).connectedAnchor = set.joint.connectedAnchor;
            ((CharacterJointData)set.savedJoint).autoConfigureConnectedAnchor = false;
            k.koboldBodyRenderers.AddRange(set.dick.deformationTargets);
            if (set.balls != null) {
                k.balls.Add(set.balls.container);
            }
            k.activeDicks.Add(set);
            set.dick.OnMove.AddListener(OnDickMovement);
            // Make sure the dick is the right color, this just forces a reset of the colors.
            k.HueBrightnessContrastSaturation = k.HueBrightnessContrastSaturation;
            StartCoroutine(UnfuckJoints(set, set.parentTransform.GetComponentInParent<Rigidbody>()));
        }
        foreach(DickSet set in dicks) {
            foreach(JigglePhysics.JiggleBone bone in set.dick.GetComponentsInChildren<JigglePhysics.JiggleBone>()) {
                bone.enabled = true;
            }
        }
        k.transform.rotation = oldRot;
        k.animator.enabled = animatorWasEnabled;
        k.lodLevel.OnLODClose.AddListener(OnDickLODClose);
        k.lodLevel.OnLODFar.AddListener(OnDickLODFar);
        k.RagdollEvent += RagdollEvent;
        RagdollEvent(k.ragdolled);
    }
    public void Update() {
        /*if (attachedKobold == null || attachedKobold.ragdolled) {
            return;
        }
        foreach (var dickSet in dicks) {
            if (dickSet.joint == null) {
                continue;
            }
            Vector3 dickForward = dickSet.dick.ragdollBody.rotation * dickSet.dick.dickForward;
            Vector3 dickUp = dickSet.dick.ragdollBody.rotation * dickSet.dick.dickUp;
            Vector3 dickRight = Vector3.Cross(dickUp, dickForward);

            //Transform parent = attachedKobold.animator.GetBoneTransform(dickSet.parent);

            Vector3 hipUp = dickSet.parentTransform.TransformDirection(dickSet.initialDickUpHipSpace);
            Vector3 hipForward = dickSet.parentTransform.TransformDirection(dickSet.initialDickForwardHipSpace);
            Vector3 hipRight = Vector3.Cross(hipUp, hipForward);

            float angleDiff = Vector3.Angle(dickForward, hipForward);

            float maxDeflection = 30f;
            float correction = Mathf.Max(angleDiff-maxDeflection, 0f);
            if (correction > 0f) {
                dickSet.dick.dickRoot.rotation = Quaternion.RotateTowards(dickSet.dick.dickRoot.rotation, Quaternion.FromToRotation(dickForward, hipForward) * dickSet.dick.dickRoot.rotation, correction);
            }
        }*/
    }
    public IEnumerator UnfuckJoints(DickSet dickSet, Rigidbody targetBody) {
        //yield return new WaitForSeconds(waitTime);
        // We first remove the original joint, we have to wait at least a frame for it to truely be gone though!
        // FIXME: Unity bug causes the joint to just... not get removed! this mess tries everything in its power to remove it.
        //while (dickSet.dick.body.GetComponent<ConfigurableJoint>() != null) {
            //Destroy(dickSet.dick.body.GetComponent<ConfigurableJoint>());
            //yield return new WaitForEndOfFrame();
        //}

        while (dickSet.dick.body.GetComponent<CharacterJoint>() != null) {
            Destroy(dickSet.dick.body.GetComponent<CharacterJoint>());
            yield return null;
        }
        while (dickSet.joint != null) {
            Destroy(dickSet.joint);
            yield return null;
        }
        dickSet.dick.body.position = dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition);
        dickSet.dick.body.rotation = dickSet.parentTransform.rotation * dickSet.initialBodyLocalRotation;
        dickSet.dick.dickRoot.position = dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition);
        dickSet.dick.dickRoot.rotation = dickSet.parentTransform.rotation * dickSet.initialTransformLocalRotation;
        Physics.SyncTransforms();
        // Finally recreate the joint.
        dickSet.joint = dickSet.savedJoint.Apply(targetBody);
        // The savedJoint probably doesn't have the right connected anchor anymore, so we update it just in-case.
        dickSet.joint.connectedAnchor = dickSet.joint.connectedBody.transform.InverseTransformPoint(dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition));
        if (attachedKobold != null) {
            if (targetBody == attachedKobold.body) {
                dickSet.joint.massScale = 100f;
            } else {
                dickSet.joint.massScale = 10f;
            }
        }
    }
    public void RagdollEvent(bool ragdolled) {
        // Ragdoll event can be triggered during destruction-- which sometimes results in the animator returning a null GetBoneTransform.
        if (attachedKobold == null || attachedKobold.animator == null) {
            return;
        }
        if (ragdolled) {
            attachedKobold.animator.enabled = true;
        }
        foreach (DickSet set in dicks) {
            if (set.parentTransform == null) {
                continue;
            }
            //Rigidbody ragdollRigidbody = attachedKobold.animator.GetBoneTransform(set.parent).GetComponentInParent<Rigidbody>();
            if (ragdolled) {
                //set.dick.koboldBody = set.parentTransform.GetComponentInParent<Rigidbody>();
                set.dick.body.isKinematic = false;
                if (set.dick == null) {
                    continue;
                }
                foreach(Collider c in set.dick.GetComponentsInChildren<Collider>()) {
                    Physics.IgnoreCollision(c,attachedKobold.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).GetComponentInChildren<Collider>());
                    Physics.IgnoreCollision(c,attachedKobold.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).GetComponentInChildren<Collider>());
                }
                StartCoroutine(UnfuckJoints(set, set.parentTransform.GetComponentInParent<Rigidbody>()));
            } else {
                //Destroy(set.joint);
                set.dick.body.isKinematic = true;
                //set.dick.koboldBody = attachedKobold.body;
                set.dick.body.transform.localPosition = set.initialBodyLocalPosition;
                set.dickContainer.transform.localPosition = -set.attachPosition;
                set.dickContainer.transform.localRotation = Quaternion.identity;
                set.dick.dickRoot.transform.localPosition = set.dickAttachPosition;
                set.dick.dickRoot.transform.localRotation = set.initialTransformLocalRotation;
            }
        }
        if (ragdolled) {
            attachedKobold.animator.enabled = false;
        }
    }
}
