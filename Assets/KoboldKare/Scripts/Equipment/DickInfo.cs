using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PenetrationTech;
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
        attachedKobold = k;
        // We need to make sure that our model isn't disabled, otherwise k.animator.GetBoneTransform always returns null :weary:
        GenericLODConsumer consumer = k.GetComponentInChildren<GenericLODConsumer>(true);
        bool wasClose = consumer.isClose;
        bool wasVeryFar = consumer.isVeryFar;
        consumer.SetVeryFar(false);
        consumer.SetClose(true);
        //transform.localPosition = Vector3.zero;
        //transform.localRotation = Quaternion.identity;
        foreach(DickSet set in dicks) {
            set.info = this;
            Vector3 scale = set.dickContainer.localScale;
            set.dick.OnCumEmit.AddListener(()=>{
                ReagentContents cumbucket = new ReagentContents();
                cumbucket.Mix(set.balls.container.contents.Spill(set.balls.container.maxVolume/set.dick.cumPulseCount));
                cumbucket.Mix(ReagentData.ID.Cum, set.dick.dickRoot.transform.lossyScale.x);
                if (!set.dick.IsInside()) {
                    set.dick.GetComponentInChildren<FluidOutput>().Fire(cumbucket, 2f);
                } else {
                    set.dick.holeTarget.GetComponentInParent<Kobold>().bellies[0].container.contents.Mix(cumbucket);
                }
            });
            //set.dick.OnPenetrate.AddListener(()=>{
            //});
            //set.dick.OnEndPenetrate.AddListener(()=>{
            //});
            set.parentTransform = k.animator.GetBoneTransform(set.parent);
            Quaternion savedRotation = set.dick.body.transform.localRotation;
            set.dick.body.transform.localRotation = Quaternion.identity;
            set.rotJoint = set.dick.body.gameObject.AddComponent<ConfigurableJoint>();
            set.rotJoint.connectedBody = set.parentTransform.GetComponentInParent<Rigidbody>();
            set.rotJoint.anchor = Vector3.zero;
            set.rotJoint.connectedAnchor = Vector3.zero;
            set.dick.body.transform.localRotation = savedRotation;
            var slerpd = set.rotJoint.slerpDrive;
            slerpd.positionSpring = 1000f;
            set.rotJoint.slerpDrive = slerpd;
            set.rotJoint.rotationDriveMode = RotationDriveMode.Slerp;

            set.dick.root = k.transform;
            set.dickContainer.parent = k.attachPoints[(int)set.attachPoint];
            set.dickContainer.localScale = scale;
            var observables = set.dickContainer.GetComponentsInChildren<IPunObservable>(true);
            for (int i = 0; i < observables.Length; i++) {
                k.photonView.ObservedComponents.Add((Component)observables[i]);
            }
            //set.dick.transform.parent = k.attachPoints[(int)Equipment.EquipmentSlot.Crotch];
            set.dickContainer.transform.localPosition = -set.attachPosition;
            set.dickContainer.transform.localRotation = Quaternion.identity;

            if (set.parent == HumanBodyBones.Hips) {
                foreach(var hole in attachedKobold.penetratables) {
                    if (hole.isFemaleExclusiveAnatomy) {
                        hole.penetratable.gameObject.SetActive(false);
                    }
                }
            }
            //set.dick.kobold = k;
            /*if (!k.ragdolled) {
                set.dick.body = attachedKobold.body;
            } else {
                set.dick.body = set.parentTransform.GetComponentInParent<Rigidbody>();
            }*/
            set.initialDickForwardHipSpace = set.parentTransform.InverseTransformDirection(set.dick.body.transform.forward);
            set.initialDickUpHipSpace = set.parentTransform.InverseTransformDirection(set.dick.body.transform.up);
            set.initialBodyLocalRotation = set.dick.body.transform.localRotation;
            set.initialBodyLocalPosition = set.dick.body.transform.localPosition;
            set.dickAttachPosition = set.parentTransform.InverseTransformPoint(set.dick.dickRoot.position);
            set.initialTransformLocalRotation = set.dick.dickRoot.localRotation;
            //dickSet.joint.axis = Vector3.up;
            //dickSet.joint.secondaryAxis = Vector3.forward;
            //dickSet.dick.body.transform.parent = root;
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
            foreach(Rigidbody r in k.ragdollBodies) {
                if (r.GetComponent<Collider>() == null) {
                    continue;
                }
                //foreach (Collider b in set.dick.selfColliders) {
                    //Physics.IgnoreCollision(r.GetComponent<Collider>(), b, true);
                //}
            }
            if (!k.ragdolled) {
                set.dick.body.isKinematic = true;
                Destroy(set.joint);
            } else if (wasClose) {
                set.dick.body.isKinematic = false;
                StartCoroutine(UnfuckJoints(set, k.body));
            }
            // Make sure the dick is the right color, this just forces a reset of the colors.
            k.HueBrightnessContrastSaturation = k.HueBrightnessContrastSaturation;
        }
        k.lodLevel.OnLODClose.AddListener(OnDickLODClose);
        k.lodLevel.OnLODFar.AddListener(OnDickLODFar);
        //k.OnOrgasm.AddListener(Cum);
        k.RagdollEvent += RagdollEvent;
        consumer.SetClose(wasClose);
        consumer.SetVeryFar(wasVeryFar);
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
    public void FixedUpdate() {
        if (attachedKobold == null) {
            return;
        }
        foreach (var dickSet in dicks) {
            if (dickSet.joint == null || dickSet.dick.body.isKinematic) {
                continue;
            }
            //dickSet.dick.ragdollBody.velocity = attachedKobold.body.velocity;
            //dickSet.dick.body.centerOfMass = dickSet.dick.body.transform.InverseTransformPoint(dickSet.dick.dickTransform.position);
            Vector3 dickForward = dickSet.dick.body.rotation * dickSet.dick.dickForward;
            Vector3 dickUp = dickSet.dick.body.rotation * dickSet.dick.dickUp;
            Vector3 dickRight = Vector3.Cross(dickUp, dickForward);

            Vector3 hipUp = dickSet.parentTransform.TransformDirection(dickSet.initialDickUpHipSpace);
            Vector3 hipForward = dickSet.parentTransform.TransformDirection(dickSet.initialDickForwardHipSpace);
            Vector3 hipRight = Vector3.Cross(hipUp, hipForward);

            if (!attachedKobold.ragdolled) { // If we're not ragdolled
                dickSet.dick.body.interpolation = attachedKobold.body.interpolation;
                dickSet.joint.connectedAnchor = dickSet.joint.connectedBody.transform.InverseTransformPoint(dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition));
            } else {
                dickSet.dick.body.interpolation = dickSet.joint.connectedBody.interpolation;
            }

            //float neededSuperCorrectionForce = Mathf.Clamp01(attachedKobold.body.velocity.GroundVector().magnitude / 4f);

            // We cannot use this garbage because ragdolls kill all rotation.
            //Vector3 cross = Vector3.Cross(dickForward, hipForward);
            //float angleDiff = Vector3.Angle(dickForward, hipForward);
            //dickSet.dick.body.AddTorque(cross * angleDiff*2f);
//
            //dickSet.dick.body.angularVelocity *= 0.9f;

            // Twist the dick to be upright (this is really important).
            // Make the dick mostly face forward using a constraint instead!
            /*Quaternion adjust = Quaternion.FromToRotation(dickForward, hipForward);
            Vector3 targetRot = dickUp;
            if (Vector3.Dot(dickUp, hipUp) < 0f) {
                targetRot = -dickUp;
            }
            adjust = adjust * Quaternion.FromToRotation(dickUp, Vector3.ProjectOnPlane(targetRot, hipRight).normalized);*/
            dickSet.rotJoint.targetRotation = Quaternion.Inverse(dickSet.initialBodyLocalRotation);
            //Vector3 ucross = Vector3.Cross(dickUp, Vector3.ProjectOnPlane(targetRot, hipRight).normalized);
            //float uangleDiff = Vector3.Angle(dickUp, Vector3.ProjectOnPlane(targetRot, hipRight).normalized);
            //dickSet.dick.body.AddTorque(ucross * uangleDiff * 0.2f);

            //if (!attachedKobold.ragdolled) {
                //dickSet.joint.massScale = Mathf.Lerp(1f, 500f, neededSuperCorrectionForce);
            //}

            //float maxDeflection = 30f;
            //float correction = Mathf.Max(angleDiff-maxDeflection, 0f);
            //if (correction > 0f) {
                //dickSet.dick.body.rotation = Quaternion.RotateTowards(dickSet.dick.body.rotation, Quaternion.FromToRotation(dickForward, hipForward) * dickSet.dick.body.rotation, correction);
            //}
        }
    }
    public IEnumerator UnfuckPenetrate(DickSet set) {
        Destroy(set.joint);
        yield return null;
        set.dick.body.isKinematic = true;
        set.dickContainer.transform.localPosition = -set.attachPosition;
        set.dickContainer.transform.localRotation = Quaternion.identity;
        //set.dick.dickRoot.transform.localPosition = set.dickAttachPosition;
        //set.dick.dickRoot.transform.localRotation = set.initialTransformLocalRotation;
        set.dick.body.position = set.parentTransform.TransformPoint(set.dickAttachPosition);
        set.dick.body.rotation = set.parentTransform.rotation * set.initialBodyLocalRotation;
        Physics.SyncTransforms();
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
        // If we're really far away, we're a 2d image! Also animator.GetBoneTransform returns null because fuck me i guess.
        if (attachedKobold.GetComponentInChildren<GenericLODConsumer>(true).isVeryFar) {
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
                foreach(Collider c in set.dick.GetComponentsInChildren<Collider>()) {
                    Physics.IgnoreCollision(c,attachedKobold.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).GetComponentInChildren<Collider>());
                    Physics.IgnoreCollision(c,attachedKobold.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).GetComponentInChildren<Collider>());
                }
                StartCoroutine(UnfuckJoints(set, set.parentTransform.GetComponentInParent<Rigidbody>()));
            } else {
                //Destroy(set.joint);
                set.dick.body.isKinematic = true;
                //set.dick.koboldBody = attachedKobold.body;
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
