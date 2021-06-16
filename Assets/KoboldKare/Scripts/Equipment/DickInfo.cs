using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
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
        public Dick dick;
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
            set.dickContainer.parent = k.attachPoints[(int)set.attachPoint];
            set.dickContainer.localScale = scale;
            var observables = set.dickContainer.GetComponentsInChildren<IPunObservable>(true);
            for (int i = 0; i < observables.Length; i++) {
                k.photonView.ObservedComponents.Add((Component)observables[i]);
            }
            //set.dick.transform.parent = k.attachPoints[(int)Equipment.EquipmentSlot.Crotch];
            set.dickContainer.transform.localPosition = -set.attachPosition;
            set.dickContainer.transform.localRotation = Quaternion.identity;

            set.parentTransform = k.animator.GetBoneTransform(set.parent);
            if (set.parent == HumanBodyBones.Hips) {
                foreach(var hole in attachedKobold.penetratables) {
                    if (hole.isFemaleExclusiveAnatomy) {
                        hole.penetratable.gameObject.SetActive(false);
                    }
                }
            }
            set.dick.kobold = k;
            if (!k.ragdolled) {
                set.dick.koboldBody = attachedKobold.body;
            } else {
                set.dick.koboldBody = set.parentTransform.GetComponentInParent<Rigidbody>();
            }
            set.initialDickForwardHipSpace = set.parentTransform.InverseTransformDirection(set.dick.dickRoot.TransformDirection(set.dick.dickForward));
            set.initialDickUpHipSpace = set.parentTransform.InverseTransformDirection(set.dick.dickRoot.TransformDirection(set.dick.dickUp));
            set.initialBodyLocalRotation = set.dick.ragdollBody.transform.localRotation;
            set.initialBodyLocalPosition = set.dick.ragdollBody.transform.localPosition;
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
                set.dick.ragdollBody.isKinematic = true;
                Destroy(set.joint);
            } else if (wasClose) {
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
            if (set.dick.ragdollBody != null && !set.dick.ragdollBody.isKinematic) {
                set.dick.ragdollBody.interpolation = RigidbodyInterpolation.Interpolate;
                set.dick.ragdollBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }
    }
    public void OnDickLODFar() {
        foreach (DickSet set in dicks) {
            if (set.dick.ragdollBody != null && !set.dick.ragdollBody.isKinematic) {
                set.dick.ragdollBody.interpolation = RigidbodyInterpolation.None;
                set.dick.ragdollBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
        }
    }
    public void OnDickMovement(float movementAmount) {
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
            if (dickSet.joint == null || dickSet.dick.ragdollBody.isKinematic) {
                continue;
            }
            //dickSet.dick.ragdollBody.velocity = attachedKobold.body.velocity;
            //dickSet.dick.body.centerOfMass = dickSet.dick.body.transform.InverseTransformPoint(dickSet.dick.dickTransform.position);
            Vector3 dickForward = dickSet.dick.ragdollBody.rotation * dickSet.dick.dickForward;
            Vector3 dickUp = dickSet.dick.ragdollBody.rotation * dickSet.dick.dickUp;
            Vector3 dickRight = Vector3.Cross(dickUp, dickForward);

            Vector3 hipUp = dickSet.parentTransform.TransformDirection(dickSet.initialDickUpHipSpace);
            Vector3 hipForward = dickSet.parentTransform.TransformDirection(dickSet.initialDickForwardHipSpace);
            Vector3 hipRight = Vector3.Cross(hipUp, hipForward);

            if (!attachedKobold.ragdolled) { // If we're not ragdolled
                dickSet.dick.ragdollBody.interpolation = attachedKobold.body.interpolation;
                dickSet.joint.connectedAnchor = dickSet.joint.connectedBody.transform.InverseTransformPoint(dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition));
            } else {
                dickSet.dick.ragdollBody.interpolation = dickSet.joint.connectedBody.interpolation;
            }

            float neededSuperCorrectionForce = Mathf.Clamp01(attachedKobold.body.velocity.GroundVector().magnitude / 4f);

            // Make the dick mostly face forward
            Vector3 cross = Vector3.Cross(dickForward, hipForward);
            float angleDiff = Vector3.Angle(dickForward, hipForward);
            dickSet.dick.ragdollBody.AddTorque(cross * angleDiff*2f);

            // Dampen, but only on dick twist.
            //dickSet.dick.body.angularVelocity = Vector3.Lerp(dickSet.dick.body.angularVelocity, Vector3.ProjectOnPlane(dickSet.dick.body.angularVelocity,dickForward), 0.5f);

            // Twist the dick to be upright (this is really important).
            Vector3 targetRot = dickUp;
            if (Vector3.Dot(dickUp, hipUp) < 0f) {
                targetRot = -dickUp;
            }
            Vector3 ucross = Vector3.Cross(dickUp, Vector3.ProjectOnPlane(targetRot, hipRight).normalized);
            float uangleDiff = Vector3.Angle(dickUp, Vector3.ProjectOnPlane(targetRot, hipRight).normalized);
            dickSet.dick.ragdollBody.AddTorque(ucross * uangleDiff * 0.2f);

            if (!attachedKobold.ragdolled) {
                dickSet.joint.massScale = Mathf.Lerp(1f, 500f, neededSuperCorrectionForce);
            }

            //float maxDeflection = 30f;
            //float correction = Mathf.Max(angleDiff-maxDeflection, 0f);
            //if (correction > 0f) {
                //dickSet.dick.body.rotation = Quaternion.RotateTowards(dickSet.dick.body.rotation, Quaternion.FromToRotation(dickForward, hipForward) * dickSet.dick.body.rotation, correction);
            //}
        }
    }
    public IEnumerator UnfuckJoints(DickSet dickSet, Rigidbody targetBody, float waitTime = 0f) {
        yield return new WaitForSeconds(waitTime);
        // We first remove the original joint, we have to wait at least a frame for it to truely be gone though!
        // FIXME: Unity bug causes the joint to just... not get removed! this mess tries everything in its power to remove it.
        //while (dickSet.dick.body.GetComponent<ConfigurableJoint>() != null) {
            //Destroy(dickSet.dick.body.GetComponent<ConfigurableJoint>());
            //yield return new WaitForEndOfFrame();
        //}

        while (dickSet.dick.ragdollBody.GetComponent<CharacterJoint>() != null) {
            Destroy(dickSet.dick.ragdollBody.GetComponent<CharacterJoint>());
            yield return new WaitForEndOfFrame();
        }
        while (dickSet.joint != null) {
            Destroy(dickSet.joint);
            yield return new WaitForEndOfFrame();
        }
        //Transform parent = attachedKobold.animator.GetBoneTransform(dickSet.parent);
        // We also use Rigidbody.position, and Rigidbody.rotation. This takes a full fixed update cycle, and only at the very end of the fixed update do the rigidbodies finish interpolating.
        dickSet.dick.ragdollBody.position = dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition);
        dickSet.dick.ragdollBody.rotation = dickSet.parentTransform.rotation * dickSet.initialBodyLocalRotation;
        yield return new WaitForFixedUpdate();
        // Force the transforms into the right position right before we recreate configurableJoint, this is just in-case, might not actually be necessary.
        dickSet.dick.ragdollBody.position = dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition);
        dickSet.dick.ragdollBody.rotation = dickSet.parentTransform.rotation * dickSet.initialBodyLocalRotation;
        dickSet.dick.dickRoot.position = dickSet.parentTransform.TransformPoint(dickSet.dickAttachPosition);
        dickSet.dick.dickRoot.rotation = dickSet.parentTransform.rotation * dickSet.initialTransformLocalRotation;
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
        foreach (DickSet set in dicks) {
            if (set.parentTransform == null) {
                continue;
            }
            //Rigidbody ragdollRigidbody = attachedKobold.animator.GetBoneTransform(set.parent).GetComponentInParent<Rigidbody>();
            if (ragdolled) {
                set.dick.koboldBody = set.parentTransform.GetComponentInParent<Rigidbody>();
                set.dick.ragdollBody.isKinematic = false;
                StartCoroutine(UnfuckJoints(set, set.parentTransform.GetComponentInParent<Rigidbody>()));
            } else {
                //Destroy(set.joint);
                set.dick.ragdollBody.isKinematic = true;
                set.dick.koboldBody = attachedKobold.body;
                set.dickContainer.transform.localPosition = -set.attachPosition;
                set.dickContainer.transform.localRotation = Quaternion.identity;
                set.dick.dickRoot.transform.localPosition = set.dickAttachPosition;
                set.dick.dickRoot.transform.localRotation = set.initialTransformLocalRotation;
            }
        }
    }
}
