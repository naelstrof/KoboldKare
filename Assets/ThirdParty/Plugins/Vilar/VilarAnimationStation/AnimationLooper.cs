using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vilar.AnimationStation {
	
	[System.Serializable]
	public class AnimationLooper {

		public const int TARGET_COUNT = 10;

		public enum TargetType { NONE, HEAD, HIP, HANDLEFT, HANDRIGHT, FOOTLEFT, FOOTRIGHT, ELBOWLEFT, ELBOWRIGHT, KNEELEFT, KNEERIGHT }

		public static string GetTypeName(int index) {
			return System.Enum.GetNames(typeof(TargetType)) [index];
		}

		public float speed;
		public TargetType[] attachments = new TargetType[TARGET_COUNT];
		public AnimationStation[] attachmentTargets = new AnimationStation[TARGET_COUNT];
		public float[] motionScale = new float[TARGET_COUNT];
		public float[] motionOffset = new float[TARGET_COUNT];
		public AnimationMotion[] motion = new AnimationMotion[TARGET_COUNT];
		public Vector3[] targetPositions = new Vector3[TARGET_COUNT];
		public Quaternion[] targetRotations = new Quaternion[TARGET_COUNT];
		public Vector3[] computedTargetPositions = new Vector3[TARGET_COUNT];
		public Vector3[] computedTargetVelocities = new Vector3[TARGET_COUNT];
		public Quaternion[] computedTargetRotations = new Quaternion[TARGET_COUNT];

		public AnimationLooper(AnimationLooper copy) {
			speed = copy.speed;
			for (int i = 0; i < TARGET_COUNT; i++) {
				attachments[i] = copy.attachments[i];
				targetPositions[i] = copy.targetPositions[i];
				targetRotations[i] = copy.targetRotations[i];
				computedTargetPositions[i] = copy.computedTargetPositions[i];
				computedTargetRotations[i] = copy.computedTargetRotations[i];
				computedTargetPositions[i] = Vector3.zero;
				computedTargetRotations[i] = Quaternion.identity;
				motion[i] = copy.motion[i];
				motionScale[i] = copy.motionScale[i];
				motionOffset[i] = copy.motionOffset[i];
			}
        }
		public AnimationLooper() {
			speed = 1f;
			for (int i = 0; i < TARGET_COUNT; i++) {
				attachments[i] = TargetType.NONE;
				targetPositions[i] = Vector3.zero;
				targetRotations[i] = Quaternion.identity;
				computedTargetPositions[i] = Vector3.zero;
				computedTargetRotations[i] = Quaternion.identity;
				motionScale[i] = 1f;
				motionOffset[i] = 0f;
			}
		}

		//public void CopyData(AnimationLooper source) {
			//speed = source.speed;
			//for (int i = 0; i < TARGET_COUNT; i++) {
				//attachments[i] = source.attachments[i];
				//targetPositions[i] = source.targetPositions[i];
				//targetRotations[i] = source.targetRotations[i];
				//motion[i] = source.motion[i];
			//}
		//}

	}		

}

