using UnityEngine;
using System.Linq;
using System.Collections;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Generic;
using KoboldKare;
using System;


public class BodyProportion : MonoBehaviour {
	[SerializeField] private Animator humanoidAnimator;
	[SerializeField] private AnimationCurve modificationCurve; // should be log(x)

    private float internalTopBottom = 1f;
    public float topBottom {
        get {
            return internalTopBottom;
        }
        set {
            if (Mathf.Approximately(internalTopBottom, value) && initialized) {
                return;
            }
            internalTopBottom = value;
            Initialize();
        }
    }
    private float internalThickness = 1f;
    public float thickness {
        get {
            return internalThickness;
        }
        set {
            if (Mathf.Approximately(internalThickness, value) && initialized) {
                return;
            }
            internalThickness = value;
            Initialize();
        }
    }
	private static bool resourceLock = false;

	//[SerializeField] private bool initImmediately;
	private Vector3 tempVert;
	private float hipMeshScale = 1f;
	private float spineMeshScale = 1f;
	private float chestMeshScale = 1f;
	private float shoulderMeshScale = 1f;
	private float upperarmMeshScale = 1f;
	public float upperlegMeshScale = 1f;
	private float hipBoneScale = 1f;
	private float spineBoneScale = 1f;
	private float chestBoneScale = 1f;
	private float shoulderBoneScale = 1f;
	//private float lastLowerbodyScale = 1f;
	//private float bodyScale = 1f;
	private float handScale = 1f;
	private float footScale = 1f;
	[HideInInspector]
	public bool queued = false;
	public List<SkinnedMeshRenderer> targetRenderers;
	public float overallScale { get; set; } = 1f;
	//private Mesh bakeMesh;
	public Dictionary<Renderer, Mesh> originalMeshMemory = new Dictionary<Renderer, Mesh>();

	public delegate void CompletedBodyProportionAction();
	public CompletedBodyProportionAction completed;
	public List<Task> tasks = new List<Task>();
	private bool initialized = false;

	private bool running {
		get {
            for (int i=0;i<tasks.Count;i++) {
                if (tasks[i].Running) {
					return true;
                }
            }
			return false;
        }
	}

	private Transform GetEndBone(HumanBodyBones bone) {
		//Transform a = humanoidAnimator.GetBoneTransform(bone);
		switch(bone) {
			case HumanBodyBones.Hips:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.Spine);
			case HumanBodyBones.Spine:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.Chest);
			case HumanBodyBones.Chest:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.Neck);
			case HumanBodyBones.Neck:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.Head);
			case HumanBodyBones.LeftShoulder:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			case HumanBodyBones.RightShoulder:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			case HumanBodyBones.LeftUpperArm:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			case HumanBodyBones.RightUpperArm:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			case HumanBodyBones.RightLowerArm:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.RightHand);
			case HumanBodyBones.LeftLowerArm:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
			case HumanBodyBones.LeftUpperLeg:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			case HumanBodyBones.RightUpperLeg:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			case HumanBodyBones.LeftLowerLeg:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
			case HumanBodyBones.RightLowerLeg:
				return humanoidAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
			default:
				return humanoidAnimator.GetBoneTransform(bone);
		}
	}

	private void CheckIfQueued() {
		if (queued) {
			queued = false;
			Initialize();
        }
	}
	private void CheckFinished(bool manuallyStopped) {
		for (int i=0;i<tasks.Count;i++) {
			if (!tasks[i].Running) {
				tasks.RemoveAt(i);
            }
        }
		if (tasks.Count == 0) {
			completed?.Invoke();
			CheckIfQueued();
        }
    }
	public void Initialize() {
		initialized = true;
		//koboldData.Randomize();
		for(int i=0;i<tasks.Count;) {
			if (tasks[i].Running) {
				queued = true;
				return;
            }
			tasks.RemoveAt(0);
        }
		foreach (SkinnedMeshRenderer r in targetRenderers) {
			if (r == null) {continue;}
			if (!originalMeshMemory.ContainsKey(r)) {
				originalMeshMemory[r] = r.sharedMesh;
			}
            r.sharedMesh = Mesh.Instantiate(originalMeshMemory[r]);
			int[] boneDictionary = GetBoneIndices(r);
			Vector3[] boneMasks = GetBoneScaleMasks(r);
			var allBoneWeights = new List<BoneWeight1>(r.sharedMesh.GetAllBoneWeights());
			var bonesPerVertex = new List<byte>(r.sharedMesh.GetBonesPerVertex());
			var vertices = r.sharedMesh.vertices;
			var bindPoses = r.sharedMesh.bindposes;
			Task t = new Task(ScaleMeshDelayed(r, boneMasks, boneDictionary, allBoneWeights, bonesPerVertex, vertices, bindPoses));
			t.Finished += CheckFinished;
            tasks.Add(t);
		}
	}

	private void LateUpdate() {
		SetScales();
		ScaleSkeleton();
	}

	private IEnumerator ScaleMeshDelayed(SkinnedMeshRenderer r, Vector3[] boneMasks, int[] boneDictionary, List<BoneWeight1> allBoneWeights, List<byte> bonesPerVertex, Vector3[] vertices, Matrix4x4[] bindPoses) {
		while (BodyProportion.resourceLock) {
			yield return new WaitUntil(() => BodyProportion.resourceLock == false);
		}
		BodyProportion.resourceLock = true;
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.Hips, hipMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.Spine, spineMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.Chest, chestMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.Neck, 1f / Mathf.Lerp(chestMeshScale, 1f, 0.5f));
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.LeftShoulder, shoulderMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.RightShoulder, shoulderMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.LeftUpperLeg, upperlegMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.RightUpperLeg, upperlegMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.LeftLowerLeg, (upperlegMeshScale + footScale) / 2f);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.RightLowerLeg, (upperlegMeshScale + footScale) / 2f);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.LeftUpperArm, upperarmMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.RightUpperArm, upperarmMeshScale);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.LeftLowerArm, (upperarmMeshScale + handScale) / 2f);
		yield return new WaitForEndOfFrame();
        ScaleMeshAtBone(vertices, bindPoses, boneDictionary, boneMasks, bonesPerVertex, allBoneWeights, HumanBodyBones.RightLowerArm, (upperarmMeshScale + handScale) / 2f);
		yield return new WaitForEndOfFrame();
		if (r != null) {
			r.sharedMesh.vertices = vertices;
		}
		BodyProportion.resourceLock = false;
	}

	private void SetScales() {
		float limitLow(float i) => i = (i < 0f ? i * 0.4f : i);
		float limitHigh(float i) => i = (i > 0f ? i * 0.4f : i);

		hipMeshScale = modificationCurve.Evaluate(-topBottom + thickness) * 0.1f;
		hipMeshScale = limitLow(hipMeshScale) + 1f;
		spineMeshScale = modificationCurve.Evaluate(thickness) * 0.5f;
		spineMeshScale = limitLow(spineMeshScale) + 1f;
		chestMeshScale = modificationCurve.Evaluate(topBottom + thickness) * 0.1f;
		chestMeshScale = limitLow(chestMeshScale) + 1f;
		shoulderMeshScale = 1f + modificationCurve.Evaluate(topBottom + thickness) * 0.2f;
		upperarmMeshScale = modificationCurve.Evaluate(topBottom + thickness) * 0.4f;
		upperarmMeshScale = limitLow(upperarmMeshScale) + 1f;
		upperlegMeshScale = modificationCurve.Evaluate(-topBottom + thickness) * 0.3f;
		upperlegMeshScale = limitLow(upperlegMeshScale) + 1f;
		hipBoneScale = modificationCurve.Evaluate(-topBottom + thickness) * 0.1f;
		hipBoneScale = limitHigh(hipBoneScale) + 1f;
		spineBoneScale = 1f + modificationCurve.Evaluate(-thickness) * 0.2f;
		chestBoneScale = modificationCurve.Evaluate(topBottom + thickness) * 0.4f;
		chestBoneScale = limitHigh(chestBoneScale) + 1f;
		shoulderBoneScale = modificationCurve.Evaluate(topBottom + thickness) * 0.4f;
		shoulderBoneScale = limitHigh(shoulderBoneScale) + 1f;
		handScale = modificationCurve.Evaluate(topBottom) * 0.3f;
		handScale = limitLow(handScale) + 1f;
		footScale = modificationCurve.Evaluate(-topBottom) * 0.3f;
		footScale = limitLow(footScale) + 1f;
	}
    private void ScaleMeshAtBone(Vector3[] vertices, Matrix4x4[] bindPoses, int[] boneIndices, Vector3[] boneScaleMasks, List<byte> bonesPerVertex, List<BoneWeight1> allBoneWeights, HumanBodyBones bone, float scale) {
        Vector3 scaler;
        int vt = 0;
        int wt = 0;
        Vector3 tempVert;
        for (int o = 0; o < bonesPerVertex.Count; o++) {
            // Find the weight of the bone on this vertex.
            float tempWeight = 0f;
            for (int p = 0; p < bonesPerVertex[o]; p++) {
                if (allBoneWeights[wt].boneIndex == boneIndices[(int)bone]) {
                    tempWeight = allBoneWeights[wt].weight;
                    //break;
                }
                wt++;
            }
			if (tempWeight == 0f) {
                vt++;
				continue;
            }
            // Scale it
            tempVert = bindPoses[boneIndices[(int)bone]].MultiplyPoint(vertices[vt]);
            scaler = Vector3.one;
			Vector3 boneScaleMask = boneScaleMasks[(int)bone];
            scaler.x = Mathf.Lerp(1f, scale, tempWeight * boneScaleMask.x);
            scaler.y = Mathf.Lerp(1f, scale, tempWeight * boneScaleMask.y);
            scaler.z = Mathf.Lerp(1f, scale, tempWeight * boneScaleMask.z);
            tempVert = Vector3.Scale(tempVert, scaler);
            vertices[vt] = bindPoses[boneIndices[(int)bone]].inverse.MultiplyPoint(tempVert);
            vt++;
        }
    }

	public void ScaleSkeleton() {
		//transform.localScale = Vector3.one;
		if (humanoidAnimator.GetBoneTransform(HumanBodyBones.Hips) == null) {
			return;
		}
		humanoidAnimator.GetBoneTransform(HumanBodyBones.Hips).localScale = Vector3.one * hipBoneScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.Spine).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.Hips).lossyScale.x) * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.Hips).lossyScale.x) * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.RightUpperLeg).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.Hips).lossyScale.x) * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.Chest).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.Spine).lossyScale.x) * chestBoneScale * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.Neck).localScale = Vector3.one;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.Chest).lossyScale.x) * shoulderBoneScale * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.RightShoulder).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.Chest).lossyScale.x) * shoulderBoneScale * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftUpperArm).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder).lossyScale.x) * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.RightUpperArm).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftShoulder).lossyScale.x) * overallScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftHand).localScale = Vector3.one * handScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.RightHand).localScale = Vector3.one * handScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).localScale = Vector3.one * footScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.RightFoot).localScale = Vector3.one * footScale;
		humanoidAnimator.GetBoneTransform(HumanBodyBones.Head).localScale = Vector3.one * (1f / humanoidAnimator.GetBoneTransform(HumanBodyBones.Neck).lossyScale.x) * overallScale;
	}

	private int[] GetBoneIndices(SkinnedMeshRenderer r) {
		// Length of HumanBodyBones is 54
		int[] dic = new int[54];
		for (int i=0;i<54;i++) {
            Transform b = humanoidAnimator.GetBoneTransform((HumanBodyBones)i);
            for(int o=0;o<r.bones.Length;o++) {
                if ( b == r.bones[o] ) {
					dic[i] = o;
                    break;
                }
            }
        }
		return dic;
	}
	private Vector3[] GetBoneScaleMasks(SkinnedMeshRenderer r) {
		Vector3[] dic = new Vector3[54];
		for (int i=0;i<54;i++) {
			dic[i] = Vector3.one;
            Transform b = humanoidAnimator.GetBoneTransform((HumanBodyBones)i);
			Transform end = GetEndBone((HumanBodyBones)i);
			if (end == null) {
				continue;
			}
            Vector3 boneLengthMask = b.InverseTransformPoint(end.position);
            //float boneLength = boneLengthMask.magnitude;
            boneLengthMask = boneLengthMask.normalized;
            Vector3 boneScaleMask = new Vector3(1f - boneLengthMask.x, 1f - boneLengthMask.y, 1f - boneLengthMask.z);
			dic[i] = boneScaleMask;
		}
		return dic;
	}
}
