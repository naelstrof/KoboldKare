using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Vilar.IK {

#if UNITY_EDITOR
	[CustomEditor(typeof(ClassicIK))]
	public class IKSolverInspector : Editor {

		void OnSceneGUI() {
			var script = (ClassicIK) target;
			script.Update();
			script.LateUpdate();
		}

	}

    //[ExecuteAlways]
#endif
	public interface IKSolver {
        IKTargetSet targets { get; }
		void Solve();
		void Initialize();
		void SetTarget(int index, Vector3 position, Quaternion rotation);
		void ForceBlend(float value);
		void CleanUp();
    }
	public class ClassicIK : MonoBehaviour, IKSolver {

		[SerializeField] private AnimationClip tpose;
		[SerializeField] private AnimationCurve antiPop;

		public void SetAntiPopAndTPose(AnimationClip newTPose, AnimationCurve newAntiPop) {
			tpose = newTPose;
			antiPop = newAntiPop;
		}

		[HideInInspector] public IKTargetSet targets { get; set; }

		public float blendTarget;
		private float blend;

		private class CorrectedTransform {
			public Transform targetTransform;
			public Quaternion rotationCorrection;
			public Vector3 position {
				get => targetTransform.position;
				set => targetTransform.position = value;
			}
			public Vector3 localPosition => targetTransform.localPosition;
			public Quaternion rotation {
				get => targetTransform.parent.rotation * targetTransform.localRotation * rotationCorrection;
				set {
					Quaternion localSet = Quaternion.Inverse(targetTransform.parent.rotation) * value;
					targetTransform.localRotation = localSet*Quaternion.Inverse(rotationCorrection);
				}
			}
			public Vector3 forward => rotation * Vector3.forward;
			public Vector3 up => rotation * Vector3.up;
		}

		Transform hip;
		Transform spine;
		Transform chest;
		Transform neck;
		Transform head;
		Transform leftShoulder;
		Transform rightShoulder;
		Animator animator;
		float neckLength;
		float chestLength;
		float spineLength;
		float hipLength;
		float torsoLength;
		float armLength;
		Vector3 virtualHead;
		Vector3 virtualHeadLook;
		Vector3 virtualNeck;
		Vector3 virtualNeckLook;
		Vector3 virtualChest;
		Vector3 virtualChestLook;
		Vector3 virtualSpine;
		Vector3 virtualSpineLook;
		Vector3 virtualHip;
		Vector3 virtualHipLook;
		Vector3 virtualMid;
		Vector3 elbowHint;
		private CorrectedTransform rightLowerArmCorrection;
		private CorrectedTransform rightUpperArmCorrection;
		private CorrectedTransform rightHandCorrection;
		
		private CorrectedTransform leftLowerArmCorrection;
		private CorrectedTransform leftUpperArmCorrection;
		private CorrectedTransform leftHandCorrection;
		
		private CorrectedTransform leftFootCorrection;
		private CorrectedTransform rightFootCorrection;
		private CorrectedTransform leftLowerLegCorrection;
		private CorrectedTransform rightLowerLegCorrection;
		private CorrectedTransform leftUpperLegCorrection;
		private CorrectedTransform rightUpperLegCorrection;
		
		private CorrectedTransform hipCorrection;
		private CorrectedTransform spineCorrection;
		private CorrectedTransform chestCorrection;
		private CorrectedTransform neckCorrection;
		private CorrectedTransform headCorrection;

		private Quaternion GetCorrection(Transform targetTransform, Vector3 desiredForward, bool pinnedRoll = true) {
			Vector3 realForward = animator.transform.TransformDirection(desiredForward);
			Vector3 parentForward = targetTransform.parent.InverseTransformDirection(realForward);
			Quaternion fix = Quaternion.FromToRotation(targetTransform.localRotation*Vector3.forward, parentForward);
			return pinnedRoll ? Quaternion.Euler(0,fix.eulerAngles.y,0) : fix;
		}

		private void Awake() {
			//if (Application.isPlaying) { Initialize(); }
			animator = GetComponentInChildren<Animator>();
			leftFootCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.LeftFoot), Vector3.up)
			};
			rightFootCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.RightFoot), Vector3.up)
			};
			leftLowerLegCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg), Vector3.back)
			};
			rightLowerLegCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg), Vector3.back)
			};
			leftUpperLegCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg), Vector3.back)
			};
			rightUpperLegCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg), Vector3.back)
			};
			rightLowerArmCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.RightLowerArm),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.RightLowerArm), Vector3.forward),
			};
			leftLowerArmCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm), Vector3.forward),
			};
			rightUpperArmCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.RightUpperArm),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.RightUpperArm), Vector3.forward),
			};
			leftUpperArmCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), Vector3.forward),
			};
			leftHandCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.LeftHand),
				rotationCorrection = Quaternion.identity,
			};
			rightHandCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.RightHand),
				rotationCorrection = Quaternion.identity,
			};
			hipCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.Hips),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.Hips), Vector3.forward),
			};
			spineCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.Spine),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.Spine), Vector3.forward),
			};
			chestCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.Chest),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.Chest), Vector3.forward),
			};
			neckCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.Neck),
				rotationCorrection = GetCorrection(animator.GetBoneTransform(HumanBodyBones.Neck), Vector3.forward),
			};
			headCorrection = new CorrectedTransform {
				targetTransform = animator.GetBoneTransform(HumanBodyBones.Head),
				rotationCorrection = Quaternion.identity,
			};
		}

        private void OnDisable() {
			CleanUp();
        }

        public void Update() {
			//Debug.DrawLine(transform.position+Vector3.up*blend, transform.position+Vector3.up*blend+Vector3.up*1f, Color.blue);
			blend = Mathf.MoveTowards(blend, blendTarget, Time.deltaTime * 2f);
			if (Application.isPlaying) {
				//animator = GetComponent<Animator>();
				//animator.speed = 0f;
				//animator.Play("TPose", 0, 0f);
				//animator.Update(Time.deltaTime);
			}
		}

		public void LateUpdate() {
			if (Application.isPlaying && isActiveAndEnabled) Solve();
		}

		public void Solve() {
			//animator = GetComponentInChildren<Animator>();
			//animator.speed = 0f;
			//animator.Play("TPose", 0, 0f);
			//animator.SetTrigger("TPose");
			//animator.Update(Time.deltaTime);
			//animator.SetTrigger("TPose");
			if (hipCorrection == null) {
				Awake();
				return;
			}

			SolveSpine(hipCorrection, spineCorrection, chestCorrection, neckCorrection, headCorrection);
			SolveLimb(leftUpperArmCorrection, leftLowerArmCorrection, leftHandCorrection, targets.GetLocalPosition(IKTargetSet.parts.HANDLEFT), targets.GetLocalRotation(IKTargetSet.parts.HANDLEFT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWLEFT), false, false);
			SolveLimb(rightUpperArmCorrection, rightLowerArmCorrection, rightHandCorrection, targets.GetLocalPosition(IKTargetSet.parts.HANDRIGHT), targets.GetLocalRotation(IKTargetSet.parts.HANDRIGHT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWRIGHT), false, false);
			correctShoulder(animator.GetBoneTransform(HumanBodyBones.LeftShoulder), animator.GetBoneTransform(HumanBodyBones.LeftUpperArm), animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
			correctShoulder(animator.GetBoneTransform(HumanBodyBones.RightShoulder), animator.GetBoneTransform(HumanBodyBones.RightUpperArm), animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
			SolveLimb(leftUpperArmCorrection, leftLowerArmCorrection, leftHandCorrection, targets.GetLocalPosition(IKTargetSet.parts.HANDLEFT), targets.GetLocalRotation(IKTargetSet.parts.HANDLEFT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWLEFT), true, false);
			SolveLimb(rightUpperArmCorrection, rightLowerArmCorrection, rightHandCorrection, targets.GetLocalPosition(IKTargetSet.parts.HANDRIGHT), targets.GetLocalRotation(IKTargetSet.parts.HANDRIGHT), targets.GetLocalPosition(IKTargetSet.parts.ELBOWRIGHT), true, false);
			SolveLimb(leftUpperLegCorrection, leftLowerLegCorrection, leftFootCorrection, targets.GetLocalPosition(IKTargetSet.parts.FOOTLEFT), targets.GetLocalRotation(IKTargetSet.parts.FOOTLEFT), targets.GetLocalPosition(IKTargetSet.parts.KNEELEFT), true, true);
			SolveLimb(rightUpperLegCorrection, rightLowerLegCorrection, rightFootCorrection, targets.GetLocalPosition(IKTargetSet.parts.FOOTRIGHT), targets.GetLocalRotation(IKTargetSet.parts.FOOTRIGHT), targets.GetLocalPosition(IKTargetSet.parts.KNEERIGHT), true, true);
		}
		
		private void TPoseForAFrame() {
			if (animator == null){
				animator = GetComponentInChildren<Animator>();
			}
			/// ---- SLOW ----
			// To discard root motion, we just cache and reset the position/rotations
			Vector3 position = animator.transform.localPosition;
			Quaternion rotation = animator.transform.localRotation;
			tpose.SampleAnimation(animator.gameObject, 0f);
			//animator.transform.SetPositionAndRotation(position, rotation);
			animator.transform.localPosition = position;
			animator.transform.localRotation = rotation;
			// --------------
		}

		public void Initialize() {
			animator = GetComponentInChildren<Animator>();
			TPoseForAFrame();
			hip = animator.GetBoneTransform(HumanBodyBones.Hips);
			spine = animator.GetBoneTransform(HumanBodyBones.Spine);
			chest = animator.GetBoneTransform(HumanBodyBones.Chest);
			neck = animator.GetBoneTransform(HumanBodyBones.Neck);
			head = animator.GetBoneTransform(HumanBodyBones.Head);
			Vector3 cachedPosition = transform.position;
			Quaternion cachedRotation = transform.rotation;
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
			targets = new IKTargetSet(animator);
			leftShoulder = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			rightShoulder = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			recalculateSpine();
			transform.position = cachedPosition;
			transform.rotation = cachedRotation;
		}
		public void CleanUp() {
        }

		public void SetTarget(int index, Vector3 position, Quaternion rotation) {
			targets.SetTarget(index, transform.InverseTransformPoint(position), Quaternion.Inverse(transform.rotation) * rotation);
		}

		private void recalculateSpine() {
			hipLength = Vector3.Distance(spine.position, hip.position)/hip.lossyScale.y;
			spineLength = Vector3.Distance(chest.position, spine.position)/spine.lossyScale.y;
			chestLength = Vector3.Distance(neck.position, chest.position)/chest.lossyScale.y;
			neckLength = Vector3.Distance(head.position, neck.position)/neck.lossyScale.y;
		}

		private void SolveSpine(CorrectedTransform hip, CorrectedTransform spine, CorrectedTransform chest, CorrectedTransform neck, CorrectedTransform head) {
			if (targets == null) Initialize();
			float soft = 0.2f;
			hip.position = Vector3.Lerp(hip.position, transform.TransformPoint(targets.GetLocalPosition(IKTargetSet.parts.HIPS)), blend);
			// ROTATE HIPS
			virtualHip = targets.GetLocalPosition(IKTargetSet.parts.HIPS);
			virtualHead = targets.GetLocalPosition(IKTargetSet.parts.HEAD);
			virtualNeck = virtualHead + targets.GetLocalRotation(IKTargetSet.parts.HEAD) * -Vector3.up * neckLength;
			//virtualChest = Vector3.Lerp(virtualHead, virtualHip, 0.5f);
			virtualChest = Vector3.Lerp(virtualHead + targets.GetLocalRotation(IKTargetSet.parts.HEAD) * -Vector3.up * (neckLength + chestLength), virtualHip + targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.up * (hipLength + spineLength), 0.5f);
			virtualSpine = virtualHip + targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.up * hipLength;


			//Debug.DrawLine(transform.TransformPoint(virtualHip), transform.TransformPoint(virtualSpine), Color.white);
			//Debug.DrawLine(transform.TransformPoint(virtualSpine), transform.TransformPoint(virtualChest), Color.red);
			//Debug.DrawLine(transform.TransformPoint(virtualChest), transform.TransformPoint(virtualNeck), Color.white);
			//Debug.DrawLine(transform.TransformPoint(virtualNeck), transform.TransformPoint(virtualHead), Color.red);
			for (int i = 0; i < 5; i++) {
				virtualNeck = Vector3.Lerp(virtualNeck, (virtualHead + virtualChest) / 2f, soft);
				virtualChest = Vector3.Lerp(virtualChest, (virtualNeck + virtualSpine) / 2f, soft);
				//virtualSpine = Vector3.Lerp(virtualSpine, (virtualChest + virtualHip) / 2f, soft*0f);
				virtualHead = Vector3.Lerp(virtualHead, targets.GetLocalPosition(IKTargetSet.parts.HEAD), soft);
				virtualNeck = Vector3.Lerp(virtualNeck, virtualHead + (virtualNeck - virtualHead).normalized * neckLength, soft * 1.5f);
				virtualChest = Vector3.Lerp(virtualChest, virtualNeck + (virtualChest - virtualNeck).normalized * chestLength, soft);
				//virtualSpine = Vector3.Lerp(virtualSpine, virtualChest + (virtualSpine - virtualChest).normalized * spineLength, soft*0f);
				//virtualHip = Vector3.Lerp(virtualHip, virtualSpine + (virtualHip - virtualSpine).normalized * hipLength, soft);
				//virtualSpine = Vector3.Lerp(virtualSpine, virtualHip + (virtualSpine - virtualHip).normalized * hipLength, soft*0f);
				virtualChest = Vector3.Lerp(virtualChest, virtualSpine + (virtualChest - virtualSpine).normalized * spineLength, soft);
				virtualNeck = Vector3.Lerp(virtualNeck, virtualChest + (virtualNeck - virtualChest).normalized * chestLength, soft * 1.5f);
				virtualHead = Vector3.Lerp(virtualHead, virtualNeck + (virtualHead - virtualNeck).normalized * neckLength, soft);
			}

			virtualHeadLook = targets.GetLocalRotation(IKTargetSet.parts.HEAD) * Vector3.forward;
			virtualHipLook = targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.forward;
			Vector3 virtualHeadLookDown = targets.GetLocalRotation(IKTargetSet.parts.HEAD) * Vector3.down;
			Vector3 virtualHipLookDown = targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.down;

			// Fix issue when hips are facing the same direction as head to prevent spine flip (in this pose: https://e621.net/posts/2358717)
			virtualNeckLook = Vector3.Lerp(virtualHeadLook, virtualHipLook, 0.3f);
			float neckDot = Vector3.Dot((virtualHead - virtualNeck).normalized, virtualNeckLook);
			virtualNeckLook = Vector3.Lerp(virtualNeckLook, Vector3.Lerp(virtualHeadLookDown, virtualHipLookDown, 0.3f), Mathf.Clamp01(neckDot));

			virtualChestLook = Vector3.Lerp(virtualHeadLook, virtualHipLook, 0.5f);
			float chestDot = Vector3.Dot((virtualNeck - virtualChest).normalized, virtualChestLook);
			virtualChestLook = Vector3.Lerp(virtualChestLook, Vector3.Lerp(virtualHeadLookDown, virtualHipLookDown, 0.5f), Mathf.Clamp01(chestDot));

			virtualSpineLook = Vector3.Lerp(virtualHeadLook, virtualHipLook, 0.75f);
			float spineDot = Vector3.Dot((virtualChest - virtualSpine).normalized, virtualSpineLook);
			virtualSpineLook = Vector3.Lerp(virtualSpineLook, Vector3.Lerp(virtualHeadLookDown, virtualHipLookDown, 0.75f), Mathf.Clamp01(spineDot));

			//Debug.DrawLine(transform.TransformPoint(virtualSpine), transform.TransformPoint(virtualChest), Color.green);
			//Debug.DrawLine(transform.TransformPoint(virtualChest), transform.TransformPoint(virtualNeck), Color.white);
			//Debug.DrawLine(transform.TransformPoint(virtualNeck), transform.TransformPoint(virtualHead), Color.green);

			//hip.rotation = Quaternion.Slerp(
				//hip.rotation,
				//Quaternion.FromToRotation(spine.position - hip.position, transform.TransformPoint(virtualSpine) - transform.TransformPoint(virtualHip)) * hip.rotation,
				//blend
			//);
			hip.rotation = Quaternion.Slerp(
				hip.rotation,
				transform.rotation * targets.GetLocalRotation(IKTargetSet.parts.HIPS),
				blend
			);
			spine.rotation = Quaternion.Slerp(
				spine.rotation,
				Quaternion.FromToRotation(chest.position - spine.position, transform.TransformPoint(virtualChest) - transform.TransformPoint(virtualSpine)) * spine.rotation,
				blend
			);
			chest.rotation = Quaternion.Slerp(
				chest.rotation,
				Quaternion.FromToRotation(neck.position - chest.position, transform.TransformPoint(virtualNeck) - transform.TransformPoint(virtualChest)) * chest.rotation,
				blend
			);
			neck.rotation = Quaternion.Slerp(
				neck.rotation,
				Quaternion.FromToRotation(head.position - neck.position, transform.TransformPoint(virtualHead) - transform.TransformPoint(virtualNeck)) * neck.rotation,
				blend
			);
			SpineTwist();

			head.rotation = Quaternion.Slerp(
				head.rotation,
				transform.rotation * targets.GetLocalRotation(IKTargetSet.parts.HEAD),
				blend
			);
		}

		private void SpineTwist() {
			//hip.rotation = Quaternion.LookRotation(hip.up, virtualHipLook) * Quaternion.Euler(90f, 0f, 0f);
			spine.rotation = Quaternion.LookRotation(spine.up, transform.rotation * virtualSpineLook) * Quaternion.Euler(-90f, 180f, 0f);
			chest.rotation = Quaternion.LookRotation(chest.up, transform.rotation * virtualChestLook) * Quaternion.Euler(-90f, 180f, 0f);
			neck.rotation = Quaternion.LookRotation(neck.up, transform.rotation * virtualNeckLook) * Quaternion.Euler(-90f, 180f, 0f);

			// Never do this
			//head.rotation = Quaternion.LookRotation(head.up, -targets.head.forward) * Quaternion.Euler(90f, 0f, 0f);
		}

		private void SolveLimb(CorrectedTransform upper, CorrectedTransform lower, CorrectedTransform end, Vector3 position, Quaternion rotation, Vector3 hint, bool noPop, bool kneeCorrection) {
			Vector3 targetPosition = transform.TransformPoint(position);
			float upperLength = Vector3.Distance(upper.position, lower.position);
			float lowerLength = Vector3.Distance(lower.position, end.position);
			if (noPop) {
				Vector3 targetOffset = (targetPosition - upper.localPosition);
				targetPosition = upper.localPosition + targetOffset.normalized * (antiPop.Evaluate(targetOffset.magnitude / (upperLength + lowerLength)) * targetOffset.magnitude);
			}
			//upper.rotation = Quaternion.Slerp(
			//	upper.localRotation,
			//	Quaternion.FromToRotation(end.localPosition - upper.localPosition, targetPosition - upper.localPosition) * upper.localRotation,
			//	blend
			//);
			virtualMid = transform.TransformPoint(hint);
			//Debug.DrawLine(upper.position, virtualMid, Color.white);
			//Debug.DrawLine(targetPosition, virtualMid, Color.white);
			for (int i = 0; i < 10; i++) {
				virtualMid = Vector3.Lerp(virtualMid, targetPosition + (virtualMid - targetPosition).normalized * lowerLength, 0.6f);
				virtualMid = Vector3.Lerp(virtualMid, upper.position + (virtualMid - upper.position).normalized * upperLength, 0.6f);
			}
			//Debug.DrawLine(upper.position, virtualMid, Color.red);
			//Debug.DrawLine(targetPosition, virtualMid, Color.red);
			upper.rotation = Quaternion.Slerp(
				upper.rotation,
				 Quaternion.FromToRotation(lower.position - upper.position, virtualMid - upper.position) * upper.rotation,
				blend
			);

			// Knee correction
			if (kneeCorrection) {
                upper.rotation = Quaternion.FromToRotation(-upper.forward, Vector3.ProjectOnPlane((virtualMid - targetPosition), upper.up).normalized) * upper.rotation;
			}

			lower.rotation = Quaternion.Slerp(
				lower.rotation,
                Quaternion.FromToRotation(end.position - lower.position, targetPosition - virtualMid) * lower.rotation,
				blend
			);

			end.rotation = Quaternion.Slerp(
				end.rotation,
				//Quaternion.LookRotation(transform.rotation * rotation * Vector3.forward, transform.rotation * rotation * Vector3.up),
				transform.rotation * rotation,
				blend
			);
		}

		private Vector3 estimateChestForward() {
			Vector3 chestUp = Vector3.Normalize(virtualChest - virtualSpine);
			Vector3 restChestForward = Vector3.Lerp(targets.GetLocalRotation(IKTargetSet.parts.HIPS) * Vector3.forward, targets.GetLocalRotation(IKTargetSet.parts.HEAD) * Vector3.forward, 0.5f);
			Vector3 leftHandOffset = Vector3.ProjectOnPlane(targets.GetLocalPosition(IKTargetSet.parts.HANDLEFT) - virtualSpine, chestUp);
			Vector3 rightHandOffset = Vector3.ProjectOnPlane(targets.GetLocalPosition(IKTargetSet.parts.HANDRIGHT) - virtualSpine, chestUp);
			Vector3 leftHandChestBias = -Vector3.Cross(leftHandOffset.normalized, chestUp);
			Vector3 rightHandChestBias = Vector3.Cross(rightHandOffset.normalized, chestUp);
			Vector3 computedChestForward = Vector3.Lerp(leftHandChestBias, rightHandChestBias, rightHandOffset.sqrMagnitude / (leftHandOffset.sqrMagnitude + rightHandOffset.sqrMagnitude));
			computedChestForward = Vector3.Lerp(restChestForward, computedChestForward, Mathf.Clamp01(Mathf.Pow(Mathf.Max(leftHandOffset.magnitude, rightHandOffset.magnitude) / armLength, 2f)));
			//Debug.DrawLine(virtualSpine, virtualSpine + chestUp, Color.cyan);
			//Debug.DrawLine(virtualSpine, virtualSpine + restChestForward, Color.cyan);
			//Debug.DrawLine(virtualSpine, virtualSpine + leftHandChestBias, Color.cyan);
			//Debug.DrawLine(virtualSpine, virtualSpine + rightHandChestBias, Color.cyan);
			return computedChestForward;
		}

		private void correctShoulder(Transform shoulder, Transform upperArm, Transform lowerArm) {
			Vector3 relaxVector = shoulder.position - (animator.GetBoneTransform(HumanBodyBones.Neck).position);
			relaxVector = relaxVector.normalized;
			Vector3 upperArmVector = lowerArm.position - upperArm.position;
			upperArmVector = upperArmVector.normalized;
			Quaternion shoulderCorrection = Quaternion.FromToRotation(upperArm.position - shoulder.position, lowerArm.position - shoulder.position);
			shoulderCorrection = Quaternion.Lerp(shoulderCorrection, Quaternion.identity, Mathf.Clamp01(Vector3.Dot(upperArmVector, relaxVector)));
			shoulder.rotation = shoulderCorrection * shoulder.rotation;
			upperArm.rotation = Quaternion.Inverse(shoulderCorrection) * upperArm.rotation;
		}

		public void ForceBlend(float value) {
			blendTarget = value;
			blend = value;
		}

	}

}