using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naelstrof.BodyProportion {
	[RequireComponent(typeof(Animator))]
	public class BodyProportionBase : MonoBehaviour {
		protected Dictionary<HumanBodyBones, float> meshScalings;
		protected Dictionary<HumanBodyBones, float> boneScalings;
		private Animator targetAnimator;
		private class RendererIDLookup {
			public RendererIDLookup() {
				shrinkIDs = new Dictionary<HumanBodyBones, int>();
				growIDs= new Dictionary<HumanBodyBones, int>();
			}

			public Dictionary<HumanBodyBones, int> shrinkIDs;
			public Dictionary<HumanBodyBones, int> growIDs;
		}

		private Dictionary<SkinnedMeshRenderer, RendererIDLookup> blendshapeLookupTable;
		[SerializeField] private List<SkinnedMeshRenderer> targetRenderers;
		
		protected virtual void Awake() {
			boneScalings = new Dictionary<HumanBodyBones, float>();
			for (int i = (int)HumanBodyBones.Hips; i < (int)HumanBodyBones.LastBone; i++) {
				boneScalings[(HumanBodyBones)i] = 1f;
			}
			meshScalings = new Dictionary<HumanBodyBones, float>();
			for (int i = (int)HumanBodyBones.Hips; i < (int)HumanBodyBones.LastBone; i++) {
				meshScalings[(HumanBodyBones)i] = 1f;
			}
			
			blendshapeLookupTable = new Dictionary<SkinnedMeshRenderer, RendererIDLookup>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in targetRenderers) {
				blendshapeLookupTable.Add(skinnedMeshRenderer, new RendererIDLookup());
				for (int i = (int)HumanBodyBones.Hips; i < (int)HumanBodyBones.LastBone; i++) {
					if (!BodyProportionStaticSettings.HasFlag((HumanBodyBones)i,
						    BodyProportionStaticSettings.BoneFlags.Blendshape)) {
						continue;
					}
					int scaleDownIndex =
						skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex($"{((HumanBodyBones)i).ToString()}_0");
					int scaleUpIndex =
						skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex($"{((HumanBodyBones)i).ToString()}_2");
					blendshapeLookupTable[skinnedMeshRenderer].shrinkIDs.Add((HumanBodyBones)i,scaleDownIndex);
					blendshapeLookupTable[skinnedMeshRenderer].growIDs.Add((HumanBodyBones)i,scaleUpIndex);
				}
			}
			targetAnimator = GetComponent<Animator>();
		}

		// TODO: This is a ton of lookups, should cache everything... but oh god so much to cache...
		public void ScaleSkeleton() {
			for (int i = (int)HumanBodyBones.Hips; i < (int)HumanBodyBones.LastBone; i++) {
				if (!BodyProportionStaticSettings.HasFlag((HumanBodyBones)i, BodyProportionStaticSettings.BoneFlags.Scale)) {
					continue;
				}
				ScaleBone(targetAnimator, (HumanBodyBones)i, boneScalings[(HumanBodyBones)i], BodyProportionStaticSettings.HasFlag((HumanBodyBones)i, BodyProportionStaticSettings.BoneFlags.IgnoreParentScale));
			}

			for (int i = (int)HumanBodyBones.Hips; i < (int)HumanBodyBones.LastBone; i++) {
				if (!BodyProportionStaticSettings.HasFlag((HumanBodyBones)i, BodyProportionStaticSettings.BoneFlags.Blendshape)) {
					continue;
				}
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in targetRenderers) {
					int scaleDownIndex = blendshapeLookupTable[skinnedMeshRenderer].shrinkIDs[(HumanBodyBones)i];
					int scaleUpIndex = blendshapeLookupTable[skinnedMeshRenderer].growIDs[(HumanBodyBones)i];
					skinnedMeshRenderer.SetBlendShapeWeight(scaleDownIndex,
						Mathf.Max(1f - meshScalings[(HumanBodyBones)i], 0f) * 100f);
					skinnedMeshRenderer.SetBlendShapeWeight(scaleUpIndex,
						Mathf.Max(meshScalings[(HumanBodyBones)i] - 1f, 0f) * 100f);
				}
			}
		}

		void ScaleBone(Animator animator, HumanBodyBones boneID, float scale, bool ignoreParent) {
			Transform bone = animator.GetBoneTransform(boneID);
			if (bone == null) {
				return;
			}

			if (!ignoreParent) {
				bone.localScale = Vector3.one * ((1f / animator.GetParentBone(boneID).lossyScale.x) * scale);
			} else {
				bone.localScale = Vector3.one * scale;
			}
		}

		protected virtual void LateUpdate() {
			ScaleSkeleton();
		}
	}
}