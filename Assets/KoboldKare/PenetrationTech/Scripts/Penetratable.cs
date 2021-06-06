using UnityEngine;
using System.Collections;
using UnityEngine.Animations;
using System.Collections.Generic;
using UnityEngine.Events;
using KoboldKare;

namespace Naelstrof {
    public class Penetratable : MonoBehaviour {
        [System.Serializable]
        public class UnityEventFloat : UnityEvent<float> { };
        public UnityEvent OnPenetrate;
        public UnityEvent OnDepenetrate;
        public UnityEventFloat OnMove;
        private bool penetrated;
        public Vector3 holeForwardAxis = new Vector3(0, 1, 0);
        public Vector3 holeUpAxis = new Vector3(0, 0, -1);
        [HideInInspector]
        public Vector3 holeRightAxis = new Vector3(1, 0, 0);
        public List<SkinnedMeshRenderer> holeMeshes = new List<SkinnedMeshRenderer>();
        public Transform holeTransform;
        [HideInInspector]
        public List<int> pullBlendshapes = new List<int>();
        [HideInInspector]
        public List<int> pushBlendshapes = new List<int>();
        [HideInInspector]
        public List<int> expandBlendshapes = new List<int>();
        public string pullBlendshapeName = "";
        public string pushBlendshapeName = "";
        public string expandBlendshapeName = "";
        [Range(0.0000001f, 0.3f)]
        public float holeDiameter = 0.01f;
        public Naelstrof.Dick dickTarget;
        private float desiredGirth = 0f;
        [HideInInspector]
        public float realGirth = 0f;
        private GameObject worldUpObject;
        public GameObject fakeHoleGameObject;
        [HideInInspector]
        public float aimWeight;
        [Range(0f, 50f)]
        public float closeSpring = 10f;
        [Range(-0.2f, 0.2f)]
        public float sampleOffset = 0f;
        [Range(0f, 0.2f)]
        public float pullSampleOffset = 0f;
        [Range(0f, -0.2f)]
        public float pushSampleOffset = 0f;
        [Range(0f, 100f)]
        public float moveSpring = 20f;
        [Range(0f, 2f)]
        public float moveDamping = 0.5f;
        [HideInInspector]
        public float pushPullAmount = 0f;
        public GameObject connectedContainer;
        public bool concealsDick = true;
        public Rigidbody body;
        private AxisJoint physicsJoint;
        public float slopeSpringStrength = 100f;
        public float slopeSpringDamp = 1f;
        private float weightLerper;
        private float weariness = 0f;
        private float effectiveCloseSpring {
            get {
                //return Mathf.Max(closeSpring - weariness, Mathf.Clamp01((Mathf.Sin(Time.timeSinceLevelLoad*5f)-0.5f)*realGirth*10f));
                return Mathf.Max(closeSpring, Mathf.Clamp01((Mathf.Sin(Time.timeSinceLevelLoad*5f)-0.5f)*realGirth*10f));
            }
        }
        public float GetLocalHoleDiameter() {
            return holeDiameter;
        }
        public float GetWorldHoleDiameter() {
            if (holeTransform == null) {
                return 1f;
            }
            return holeDiameter * holeTransform.lossyScale.x;
        }

        public void SwitchBody(Rigidbody targetBody) {
            if (physicsJoint != null) {
                physicsJoint.body = targetBody;
            }
            body = targetBody;
        }

        public void Start() {
            pullBlendshapes.Clear();
            pushBlendshapes.Clear();
            expandBlendshapes.Clear();
            for (int i = 0; i < holeMeshes.Count; i++) {
                pullBlendshapes.Add(holeMeshes[i].sharedMesh.GetBlendShapeIndex(pullBlendshapeName));
                pushBlendshapes.Add(holeMeshes[i].sharedMesh.GetBlendShapeIndex(pushBlendshapeName));
                expandBlendshapes.Add(holeMeshes[i].sharedMesh.GetBlendShapeIndex(expandBlendshapeName));
            }
            physicsJoint = body.gameObject.AddComponent<AxisJoint>();
            physicsJoint.body = body;
            physicsJoint.weight = 0f;
            worldUpObject = new GameObject(holeTransform.name + "WorldUpAxis");
            worldUpObject.transform.parent = holeTransform.parent;
            worldUpObject.transform.position = holeTransform.position + holeTransform.TransformDirection(holeUpAxis);
            if (dickTarget && dickTarget.dickTransform) {
                physicsJoint.connectedBody = dickTarget.body;
                physicsJoint.localConnectedAxis = physicsJoint.connectedBody.transform.InverseTransformDirection(dickTarget.dickTransform.TransformDirection(dickTarget.dickForwardAxis));
                physicsJoint.localConnectedAnchor = physicsJoint.connectedBody.transform.InverseTransformPoint(dickTarget.dickTransform.position);
                physicsJoint.weight = 1f;
            }
            // The fake hole is so that dicks aim properly.
            //realHoleTransform = holeTransform;
            fakeHoleGameObject = new GameObject(holeTransform.name + "(Copy)");
            fakeHoleGameObject.transform.parent = holeTransform.parent;
            fakeHoleGameObject.transform.localPosition = holeTransform.localPosition;
            fakeHoleGameObject.transform.localRotation = holeTransform.localRotation;
            physicsJoint.localAnchor = body.transform.InverseTransformPoint(fakeHoleGameObject.transform.position);
            physicsJoint.localForwardAxis = physicsJoint.body.transform.InverseTransformDirection(fakeHoleGameObject.transform.TransformDirection(holeForwardAxis));
            //holeTransform = fakeHoleGameObject.transform;
        }
        public void OnDestroy() {
            Destroy(worldUpObject);
            Destroy(fakeHoleGameObject);
        }
        public void SetGirth(float girth) {
            desiredGirth = girth;
            //Debug.Log(desiredGirth);
            //holeTransform.position = fakeHoleGameObject.transform.position + offset;
        }
        public void AddSlideForce(float distance) {
            float localDistance = (distance / holeTransform.lossyScale.x) * 30f;
            pushPullAmount = Mathf.Clamp(pushPullAmount + localDistance, -2f, 2f);
            weariness += Mathf.Abs(localDistance / 50 * realGirth * 0.25f);
            OnMove.Invoke(Mathf.Abs(localDistance));
        }
        public Vector3 GetUnalteredSamplePosition() {
            return fakeHoleGameObject.transform.position;
        }
        public Vector3 GetSamplePosition() {
            float offset;
            if (pushPullAmount >= 0) {
                offset = Mathf.Lerp(sampleOffset, pushSampleOffset, pushPullAmount);
            } else {
                offset = Mathf.Lerp(sampleOffset, pullSampleOffset, Mathf.Abs(pushPullAmount));
            }
            Vector3 p = fakeHoleGameObject.transform.position + fakeHoleGameObject.transform.TransformDirection(holeForwardAxis) * offset;
            return p;
        }
        public void FixedUpdate() {
            if (dickTarget == null) {
                return;
            }
            Vector3 pos = fakeHoleGameObject.transform.position;
            float distance = Vector3.Distance(dickTarget.dickTransform.position, pos);
            float slope = -(0.1f+dickTarget.ScatterSampleDerivative(distance, 0.016f * holeTransform.lossyScale.x, 6));
            Vector3 dir = fakeHoleGameObject.transform.TransformDirection(holeForwardAxis);
            Vector3 force = dir * slope * slopeSpringStrength * Mathf.Clamp(realGirth * 1.5f, 0f, 6f) * dickTarget.girthForceMultiplier;
            // Try to keep it mostly going inwards.
            Vector3 dampingForce = Vector3.Project(body.velocity-dickTarget.body.velocity, dir) * slopeSpringDamp;
            dickTarget.body.velocity += dampingForce;
            body.velocity -= dampingForce;

            dickTarget.body.AddForceAtPosition(force * weightLerper, holeTransform.position);
            body.AddForceAtPosition(-force * weightLerper, holeTransform.position);
        }
        public void Update() {
            if (pushPullAmount >= 0) {
                for (int i = 0; i < holeMeshes.Count; i++) {
                    if (holeMeshes[i].sharedMesh.blendShapeCount > pullBlendshapes[i]) {
                        if (pullBlendshapes[i] == -1) {
                            continue;
                        }
                        holeMeshes[i].SetBlendShapeWeight(pullBlendshapes[i], 0f);
                    }
                    if (holeMeshes[i].sharedMesh.blendShapeCount > pushBlendshapes[i]) {
                        if (pushBlendshapes[i] == -1) {
                            continue;
                        }
                        holeMeshes[i].SetBlendShapeWeight(pushBlendshapes[i], pushPullAmount * 100f);
                    }
                }
            } else {
                for (int i = 0; i < holeMeshes.Count; i++) {
                    if (holeMeshes[i].sharedMesh.blendShapeCount > pullBlendshapes[i]) {
                        if (pullBlendshapes[i] == -1) {
                            continue;
                        }
                        holeMeshes[i].SetBlendShapeWeight(pullBlendshapes[i], Mathf.Abs(pushPullAmount) * 100f);
                    }
                    if (holeMeshes[i].sharedMesh.blendShapeCount > pushBlendshapes[i]) {
                        if (pushBlendshapes[i] == -1) {
                            continue;
                        }
                        holeMeshes[i].SetBlendShapeWeight(pushBlendshapes[i], 0f);
                    }
                }
            }
            pushPullAmount -= pushPullAmount * Time.deltaTime * 0.25f;
            Transform dickTransform;
            Vector3 dickForward;
            if (dickTarget == null || dickTarget.dickTransform == null) {
                desiredGirth = 0f;
                realGirth = Mathf.MoveTowards(Mathf.Max(realGirth, desiredGirth), desiredGirth, Mathf.Abs(realGirth - desiredGirth) * Time.deltaTime * effectiveCloseSpring);
                for (int i = 0; i < holeMeshes.Count; i++) {
                    if (holeMeshes[i].sharedMesh != null && holeMeshes[i].sharedMesh.blendShapeCount > expandBlendshapes[i]) {
                        if (expandBlendshapes[i] == -1) {
                            continue;
                        }
                        holeMeshes[i].SetBlendShapeWeight(expandBlendshapes[i], (realGirth * 0.5f / GetWorldHoleDiameter()) * 100f);
                    }
                }
                physicsJoint.weight = 0f;
                if (penetrated) {
                    penetrated = false;
                    OnDepenetrate.Invoke();
                }
                holeTransform.rotation = fakeHoleGameObject.transform.rotation;
                holeTransform.localPosition = fakeHoleGameObject.transform.localPosition;
                return;
            } else {
                dickTransform = dickTarget.dickTransform;
                dickForward = dickTransform.TransformDirection(dickTarget.dickForwardAxis);

                physicsJoint.connectedBody = dickTarget.body;
                physicsJoint.localConnectedAxis = physicsJoint.connectedBody.transform.InverseTransformDirection(dickForward);
                physicsJoint.localConnectedAnchor = physicsJoint.connectedBody.transform.InverseTransformPoint(dickTarget.dickTransform.position);
                physicsJoint.weight = 1f;
                Quaternion rotateAdjustu = Quaternion.FromToRotation(holeTransform.TransformDirection(holeUpAxis), (worldUpObject.transform.position-holeTransform.position).normalized);
                holeTransform.rotation =  rotateAdjustu * holeTransform.rotation;
                Quaternion rotateAdjustf = Quaternion.FromToRotation(holeTransform.TransformDirection(holeForwardAxis), Vector3.Lerp(fakeHoleGameObject.transform.TransformDirection(holeForwardAxis),-dickForward,0.5f));
                holeTransform.rotation =  rotateAdjustf * holeTransform.rotation;
            }
            //Vector3 centerOffset = fakeHoleGameObject.transform.position - holeTransform.position;
            physicsJoint.localAnchor = body.transform.InverseTransformPoint(fakeHoleGameObject.transform.position);
            physicsJoint.localForwardAxis = physicsJoint.body.transform.InverseTransformDirection(fakeHoleGameObject.transform.TransformDirection(holeForwardAxis));
            float length = dickTarget.GetWorldLength();
            Vector3 pos = fakeHoleGameObject.transform.position;
            float distance = Vector3.Distance(dickTarget.dickTransform.position, pos);

            //if (Vector3.Dot(pos-dickTransform.position, dickForward) < 0f) {
                //distance = 0f;
            //}

            weightLerper = Mathf.Lerp(weightLerper, aimWeight, Time.deltaTime * 6f);


            float dot = Mathf.Clamp01(Vector3.Dot((pos-dickTransform.position).normalized, fakeHoleGameObject.transform.TransformDirection(holeForwardAxis)));

            //float aimWeight = 0f;
            if (distance > length) {
                aimWeight = Mathf.Clamp01(1f - ((distance - (length)) * 25f));
            } else {
                aimWeight = 1f;
            }
            if (distance - length <= 0f && !penetrated) {
                penetrated = true;
                OnPenetrate.Invoke();
            } else if (distance - length > 0f && penetrated) {
                penetrated = false;
                OnDepenetrate.Invoke();
            }
            physicsJoint.weight = weightLerper;// * dot;
            realGirth = Mathf.MoveTowards(Mathf.Max(realGirth, desiredGirth), desiredGirth, Mathf.Abs(realGirth - desiredGirth) * Time.deltaTime * effectiveCloseSpring);
            for (int i = 0; i < holeMeshes.Count; i++) {
                if (holeMeshes[i].sharedMesh.blendShapeCount > expandBlendshapes[i]) {
                    if (expandBlendshapes[i] == -1) {
                        continue;
                    }
                    holeMeshes[i].SetBlendShapeWeight(expandBlendshapes[i], (realGirth * 0.5f / GetWorldHoleDiameter()) * 100f);
                }
            }
            //Vector3 proj = Vector3.Project(fakeHoleGameObject.transform.position - dickTransform.position, dickForward);
            //proj += dickTransform.position;
            float height = dickTarget.GetHeightSample(distance);
            Vector3 proj = dickTarget.GetXYOffsetWorld(height, dickTarget.weights);

            holeTransform.position = fakeHoleGameObject.transform.position+proj;
        }
    }
}
