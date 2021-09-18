using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Vilar.IK {
    public class PhysicsIK : MonoBehaviour, IKSolver {
        public class CustomJoint {
            public Rigidbody body;
            public Vector3 anchor;
            public Vector3 targetVelocity;
            public Vector3 targetWorldPosition;
            public float strength;
            public float damping = 0.3f;
            public bool rotationEnabled;
            public Quaternion targetRotation = Quaternion.identity;
            public ConfigurableJoint joint;
            //public Quaternion rotationAdjust;
        }
        [System.Serializable]
        public class LimbOrientation {
            public IKTargetSet.parts part;
            public Vector3 forward = Vector3.forward;
            public Vector3 up = Vector3.up;
            [HideInInspector]
            public Vector3 right = Vector3.right;
        }
        private static LimbOrientation defaultOrientation = new LimbOrientation();
        public List<LimbOrientation> orientations;
        public void AddJoint(int index, Transform body, Vector3 worldAnchor, float strength, bool rotationEnabled = true) {
            var lo = GetOrientation((IKTargetSet.parts)index);
            CustomJoint newJoint = new CustomJoint();
            newJoint.body = body.GetComponentInParent<Rigidbody>();
            newJoint.anchor = newJoint.body.transform.InverseTransformPoint(worldAnchor);
            newJoint.strength = strength;
            newJoint.rotationEnabled = rotationEnabled;
            if (rotationEnabled) {
                Quaternion savedRotation = newJoint.body.transform.rotation;
                newJoint.body.transform.rotation = Quaternion.identity;
                newJoint.joint = newJoint.body.gameObject.AddComponent<ConfigurableJoint>();
                newJoint.body.transform.rotation = savedRotation;
                var slerpd = newJoint.joint.slerpDrive;
                slerpd.positionSpring = strength*10f;
                newJoint.joint.slerpDrive = slerpd;
                newJoint.joint.rotationDriveMode = RotationDriveMode.Slerp;
                newJoint.joint.configuredInWorldSpace = true;
            }

            //LimbOrientation lo = GetOrientation((IKTargetSet.parts)index);
            //newJoint.rotationAdjust = Quaternion.Inverse(Quaternion.LookRotation(lo.forward, lo.up));
            joints[index] = newJoint;
        }
        public Animator animator;
        public Kobold kobold;
        [HideInInspector] public IKTargetSet targets { get; set; }
        private CustomJoint[] joints = new CustomJoint[10];
        public void ForceBlend(float value) {
            // Blend not supported
        }
        private bool IKEnabled = false;
        public LimbOrientation GetOrientation(IKTargetSet.parts part) {
            foreach(LimbOrientation lo in orientations) {
                if(lo.part == part) {
                    return lo;
                }
            }
            return defaultOrientation;
        }
        public void Initialize() {
            CleanUp();
            foreach(LimbOrientation lo in orientations) {
                Vector3.OrthoNormalize(ref lo.forward, ref lo.up, ref lo.right);
                //lo.forward = Quaternion.AngleAxis(-90f, lo.right) * lo.forward;
                //lo.up = Quaternion.AngleAxis(-90f, lo.right) * lo.up;
            }
            //animator.Play("TPose", 0, 0f);
            //animator.Update(0f);
            //animator.SetTrigger("UnTPose");
			//animator.ResetTrigger("TPose");
            //targets = new IKTargetSet(animator);
            float strength = 5f;
            AddJoint((int)IKTargetSet.parts.HEAD, animator.GetBoneTransform(HumanBodyBones.Head), animator.GetBoneTransform(HumanBodyBones.Head).position, strength*2f);
            AddJoint((int)IKTargetSet.parts.HANDLEFT, animator.GetBoneTransform(HumanBodyBones.LeftHand), animator.GetBoneTransform(HumanBodyBones.LeftHand).position, strength*0.5f);
            AddJoint((int)IKTargetSet.parts.ELBOWLEFT, animator.GetBoneTransform(HumanBodyBones.LeftHand).parent.parent, animator.GetBoneTransform(HumanBodyBones.LeftHand).parent.position, strength/2f, false);
            AddJoint((int)IKTargetSet.parts.HANDRIGHT, animator.GetBoneTransform(HumanBodyBones.RightHand), animator.GetBoneTransform(HumanBodyBones.RightHand).position, strength*0.5f);
            AddJoint((int)IKTargetSet.parts.ELBOWRIGHT, animator.GetBoneTransform(HumanBodyBones.RightHand).parent.parent, animator.GetBoneTransform(HumanBodyBones.RightHand).parent.position, strength/2f, false);
            AddJoint((int)IKTargetSet.parts.FOOTLEFT, animator.GetBoneTransform(HumanBodyBones.LeftFoot), animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, strength*0.5f);
            AddJoint((int)IKTargetSet.parts.KNEELEFT, animator.GetBoneTransform(HumanBodyBones.LeftFoot).parent.parent, animator.GetBoneTransform(HumanBodyBones.LeftFoot).parent.position, strength/2f, false);
            AddJoint((int)IKTargetSet.parts.FOOTRIGHT, animator.GetBoneTransform(HumanBodyBones.RightFoot), animator.GetBoneTransform(HumanBodyBones.RightFoot).position, strength*0.5f);
            AddJoint((int)IKTargetSet.parts.KNEERIGHT, animator.GetBoneTransform(HumanBodyBones.RightFoot).parent.parent, animator.GetBoneTransform(HumanBodyBones.RightFoot).parent.position, strength/2f, false);
            AddJoint((int)IKTargetSet.parts.HIPS, animator.GetBoneTransform(HumanBodyBones.Hips), animator.GetBoneTransform(HumanBodyBones.Hips).position, strength);
            kobold.KnockOver(9999999);
            IKEnabled = true;
        }

        public void CleanUp() {
            foreach(var j in joints) {
                if (j != null && j.joint != null) {
                    Destroy(j.joint);
                }
            }
            IKEnabled = false;
        }

        public void SetTarget(int index, Vector3 position, Quaternion rotation, Vector3 velocity) {
            //targets.SetTarget(index, transform.InverseTransformPoint(position), Quaternion.Inverse(transform.rotation) * rotation);
            if (joints[index] != null) {
                joints[index].targetWorldPosition = position;
                joints[index].targetRotation = rotation;
                joints[index].targetVelocity = velocity;
                //Debug.DrawLine(joints[index].GetComponent<Rigidbody>().transform.TransformPoint(joints[index].anchor), position, Color.red);
            }
        }
        public void FixedUpdate() {
            if (!IKEnabled) {
                return;
            }
            for(int i=0;i<10;i++) {
                Vector3 bodyPos = joints[i].body.transform.TransformPoint(joints[i].anchor);
                Vector3 targetPos = joints[i].targetWorldPosition;
                Vector3 linearForce = (targetPos - bodyPos)*joints[i].strength;

                var lo = GetOrientation((IKTargetSet.parts)i);
                joints[i].body.velocity *= (1f-joints[i].damping);
                float wrongDirForce = Mathf.Clamp01(-Vector3.Dot(joints[i].body.velocity.normalized, linearForce.normalized));
                joints[i].body.AddForce(linearForce-joints[i].body.velocity*wrongDirForce, ForceMode.VelocityChange);

                joints[i].body.velocity = Vector3.Lerp(joints[i].body.velocity, joints[i].targetVelocity, 0.9f);

                //Vector3 bodyForward = joints[i].body.transform.TransformDirection(lo.forward);
                //Vector3 bodyUp = joints[i].body.transform.TransformDirection(lo.up);
                //Vector3 bodyRight = joints[i].body.transform.TransformDirection(lo.right);
//
                //Vector3 targetForward = joints[i].targetRotation * Vector3.forward;
                //Vector3 targetUp = joints[i].targetRotation * Vector3.up;
                //Vector3 targetRight = joints[i].targetRotation * Vector3.right;

                /*float deflectionForgiveness = 5f;
                Vector3 axis = Vector3.Cross(bodyForward, targetForward);
                float angle = Mathf.Max(Vector3.Angle(bodyForward, targetForward)-deflectionForgiveness,0f);
                joints[i].body.angularVelocity = axis * angle * joints[i].strength * 1.5f;*/
                if (joints[i].rotationEnabled) {
                    Quaternion adjust = Quaternion.LookRotation(lo.forward, lo.up);
                    joints[i].joint.targetRotation = Quaternion.Inverse(joints[i].targetRotation * Quaternion.Inverse(adjust));
                    //joints[i].joint.targetRotation = joints[i].body.rotation;
                }
                //if (i == (int)(IKTargetSet.parts.HANDRIGHT)) {
                    //Debug.DrawLine(bodyPos, bodyPos + bodyForward, Color.blue);
                    //Debug.DrawLine(bodyPos, bodyPos + targetForward, Color.blue);
//
                    //Debug.DrawLine(bodyPos, bodyPos + bodyUp, Color.green);
                    //Debug.DrawLine(bodyPos, bodyPos + targetUp, Color.green);
//
                    //Debug.DrawLine(bodyPos, bodyPos + bodyRight, Color.red);
                    //Debug.DrawLine(bodyPos, bodyPos + targetRight, Color.red);
                //}
            }
        }

        public void Solve() {
            //Nothing to do! Hope that physics does its thing.
        }
    }
}
