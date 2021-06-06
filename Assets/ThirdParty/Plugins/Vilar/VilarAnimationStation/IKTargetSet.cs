using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Vilar.IK {

	[System.Serializable]
	public class IKTargetSet {

		public enum parts { HEAD, HIPS, HANDLEFT, HANDRIGHT, FOOTLEFT, FOOTRIGHT, ELBOWRIGHT, ELBOWLEFT, KNEERIGHT, KNEELEFT }

		public Matrix4x4[] targets;
		public Matrix4x4[] anchors;

		public IKTargetSet(Animator animator) {
			targets = new Matrix4x4[System.Enum.GetValues(typeof(parts)).Length];
			anchors = new Matrix4x4[System.Enum.GetValues(typeof(parts)).Length];

			Matrix4x4 localRoot = CreateMatrixFromLocalTransform(animator.transform, Matrix4x4.identity);

			targets[(int)parts.HEAD] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Head), localRoot, Quaternion.identity);
			targets[(int)parts.HIPS] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Hips), localRoot, Quaternion.identity);
			targets[(int)parts.HANDLEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand), localRoot, Quaternion.Euler(0, -90f, 0f));
			targets[(int)parts.HANDRIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightHand), localRoot, Quaternion.Euler(0, 90f, 0f));
			targets[(int)parts.FOOTLEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftFoot), localRoot, Quaternion.identity);
			targets[(int)parts.FOOTRIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightFoot), localRoot, Quaternion.identity);
			targets[(int)parts.ELBOWLEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), localRoot, Quaternion.Euler(0, -90f, 0f));
			targets[(int)parts.ELBOWRIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), localRoot, Quaternion.Euler(0, 90f, 0f));
			targets[(int)parts.KNEELEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), localRoot, Quaternion.identity);
			targets[(int)parts.KNEERIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), localRoot, Quaternion.identity);

			anchors[(int)parts.HEAD] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Head), localRoot * targets[(int)parts.HEAD]);
			anchors[(int)parts.HIPS] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Hips), localRoot * targets[(int)parts.HIPS]);
			anchors[(int)parts.HANDLEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand), localRoot * targets[(int)parts.HANDLEFT]);
			anchors[(int)parts.HANDRIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightHand), localRoot * targets[(int)parts.HANDRIGHT]);
			anchors[(int)parts.FOOTLEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftFoot), localRoot * targets[(int)parts.FOOTLEFT]);
			anchors[(int)parts.FOOTRIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightFoot), localRoot * targets[(int)parts.FOOTRIGHT]);
			anchors[(int)parts.ELBOWLEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), localRoot * targets[(int)parts.ELBOWLEFT]);
			anchors[(int)parts.ELBOWRIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), localRoot * targets[(int)parts.ELBOWRIGHT]);
			anchors[(int)parts.KNEELEFT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), localRoot * targets[(int)parts.KNEELEFT]);
			anchors[(int)parts.KNEERIGHT] = CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), localRoot * targets[(int)parts.KNEERIGHT]);
		}

		public void SetTarget(int index, Vector3 position, Quaternion rotation) {
			targets[index] = Matrix4x4.TRS(position, rotation.normalized, Vector3.one);
		}

		public Vector3 GetLocalPosition(parts part) { return GetLocalPosition((int)part); }
		public Vector3 GetLocalPosition(int part) {
			return targets[part] * anchors[part] * new Vector4(0f, 0f, 0f, 1f);
		}

		public Quaternion GetLocalRotation(parts part) { return GetLocalRotation((int)part); }
		public Quaternion GetLocalRotation(int part) {
			return (targets[part] * anchors[part]).rotation;
		}

		private Matrix4x4 CreateMatrixFromLocalTransform(Transform t, Matrix4x4 parent) {
			//parent[0] = parent[5] = parent[10] = parent[15] = 1f; // SET SCALE TO NEUTRAL
			return parent.inverse * Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
		}

		private Matrix4x4 CreateMatrixFromLocalTransform(Transform t, Matrix4x4 parent, Quaternion rotationOverride) {
			//parent[0] = parent[5] = parent[10] = parent[15] = 1f; // SET SCALE TO NEUTRAL
			return parent.inverse * Matrix4x4.TRS(t.position, rotationOverride, Vector3.one);
		}

	}

}
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VilarIK {

	[System.Serializable]
	public class IKTargetSet {

		public enum parts { HEAD, HIPS, HANDLEFT, HANDRIGHT, FOOTLEFT, FOOTRIGHT }

		public int anInt = 1;

		[SerializeField] public List<float> targets;
		[SerializeField] public List<float> anchors;

		public IKTargetSet(Animator animator) {
			targets = new List<float>();//[System.Enum.GetValues(typeof(parts)).Length * 16];
			anchors = new List<float>(); //float[System.Enum.GetValues(typeof(parts)).Length * 16];
			for (int i = 0; i < System.Enum.GetValues(typeof(parts)).Length * 16; i++) targets.Add(0f);
			for (int i = 0; i < System.Enum.GetValues(typeof(parts)).Length * 16; i++) anchors.Add(0f);

			Matrix4x4 localRoot = CreateMatrixFromLocalTransform(animator.transform, Matrix4x4.identity);

			SetTarget((int)parts.HEAD, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Head), localRoot, Quaternion.identity));
			SetTarget((int)parts.HIPS, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Hips), localRoot, Quaternion.identity));
			SetTarget((int)parts.HANDLEFT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand), localRoot, Quaternion.Euler(45f, -90f, 0f)));
			SetTarget((int)parts.HANDRIGHT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightHand), localRoot, Quaternion.Euler(45f, 90f, 0f)));
			SetTarget((int)parts.FOOTLEFT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftFoot), localRoot, Quaternion.identity));
			SetTarget((int)parts.FOOTRIGHT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightFoot), localRoot, Quaternion.identity));

			SetAnchor((int)parts.HEAD, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Head), localRoot * GetTarget((int)parts.HEAD)));
			SetAnchor((int)parts.HIPS, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.Hips), localRoot * GetTarget((int)parts.HIPS)));
			SetAnchor((int)parts.HANDLEFT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftHand), localRoot * GetTarget((int)parts.HANDLEFT)));
			SetAnchor((int)parts.HANDRIGHT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightHand), localRoot * GetTarget((int)parts.HANDRIGHT)));
			SetAnchor((int)parts.FOOTLEFT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.LeftFoot), localRoot * GetTarget((int)parts.FOOTLEFT)));
			SetAnchor((int)parts.FOOTRIGHT, CreateMatrixFromLocalTransform(animator.GetBoneTransform(HumanBodyBones.RightFoot), localRoot * GetTarget((int)parts.FOOTRIGHT)));
			Debug.Log(targets.Count);
			anInt = 2;
		}

		public void SetTarget(int index, Vector3 position, Quaternion rotation) {
			Debug.Log(anInt);
			Debug.Log(targets.Count);
			Matrix4x4 tempMatrix = Matrix4x4.TRS(position, rotation, Vector3.one);
			for (int i = 0; i < 16; i++) targets[index * 16 + i] = tempMatrix[i];
		}

		public Vector3 GetLocalPosition(parts part) { return GetLocalPosition((int)part); }
		public Vector3 GetLocalPosition(int part) {
			return targets[part] * anchors[part] * new Vector4(0f, 0f, 0f, 1f);
		}

		public Quaternion GetLocalRotation(parts part) { return GetLocalRotation((int)part); }
		public Quaternion GetLocalRotation(int part) {
			return (GetAnchor(part) * GetTarget(part)).rotation;
		}

		private void SetTarget(int index, Matrix4x4 matrix) {
			for (int i = 0; i < 16; i++) targets[index * 16 + i] = matrix[i];
		}

		private void SetAnchor(int index, Matrix4x4 matrix) {
			for (int i = 0; i < 16; i++) targets[index * 16 + i] = matrix[i];
		}

		private Matrix4x4 GetTarget(int index) {
			Matrix4x4 newMatrix = new Matrix4x4();
			for (int i = 0; i < 16; i++) newMatrix[i] = targets[index * 16 + i];
			return newMatrix;
		}

		private Matrix4x4 GetAnchor(int index) {
			Matrix4x4 newMatrix = new Matrix4x4();
			for (int i = 0; i < 16; i++) newMatrix[i] = anchors[index * 16 + i];
			return newMatrix;
		}

		private Matrix4x4 CreateMatrixFromLocalTransform(Transform t, Matrix4x4 parent) {
			//parent[0] = parent[5] = parent[10] = parent[15] = 1f;
			return parent.inverse * Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
		}

		private Matrix4x4 CreateMatrixFromLocalTransform(Transform t, Matrix4x4 parent, Quaternion rotationOverride) {
			//parent[0] = parent[5] = parent[10] = parent[15] = 1f;
			return parent.inverse * Matrix4x4.TRS(t.position, rotationOverride, Vector3.one);
		}

	}

}*/