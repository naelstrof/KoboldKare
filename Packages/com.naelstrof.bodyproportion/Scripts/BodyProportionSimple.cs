using System;
using System.Collections;
using System.Collections.Generic;
using Naelstrof.BodyProportion;
using UnityEngine;

public class BodyProportionSimple : BodyProportionBase {
	[SerializeField]
	private AnimationCurve modificationCurve;
    [SerializeField] [Range(-1f,1f)]
    private float topBottom = 0f;
    [SerializeField] [Range(-1f,1f)]
    private float thickness = 0f;

    public void SetTopBottom(float newTopBottom) {
	    topBottom = newTopBottom;
	    Apply();
    }
    public void SetThickness(float newThickness) {
	    thickness = newThickness;
	    Apply();
    }

    void Update() {
	    Apply();
    }

    void Apply() {
        float limitLow(float i) => i = (i < 0f ? i * 0.4f : i);
        float limitHigh(float i) => i = (i > 0f ? i * 0.4f : i);
        
		float hipMeshScale = modificationCurve.Evaluate(-topBottom + thickness) * 0.1f;
		hipMeshScale = limitLow(hipMeshScale) + 1f;
		meshScalings[HumanBodyBones.Hips] = hipMeshScale;
		
		float spineMeshScale = modificationCurve.Evaluate(thickness) * 0.5f;
		spineMeshScale = limitLow(spineMeshScale) + 1f;
		meshScalings[HumanBodyBones.Spine] = spineMeshScale;
		
		float chestMeshScale = modificationCurve.Evaluate(topBottom + thickness) * 0.1f;
		chestMeshScale = limitLow(chestMeshScale) + 1f;
		meshScalings[HumanBodyBones.Chest] = chestMeshScale;
		
		float shoulderMeshScale = 1f + modificationCurve.Evaluate(topBottom + thickness) * 0.2f;
		meshScalings[HumanBodyBones.LeftShoulder] = shoulderMeshScale;
		meshScalings[HumanBodyBones.RightShoulder] = shoulderMeshScale;
		
		float upperarmMeshScale = modificationCurve.Evaluate(topBottom + thickness) * 0.4f;
		upperarmMeshScale = limitLow(upperarmMeshScale) + 1f;
		meshScalings[HumanBodyBones.LeftUpperArm] = upperarmMeshScale;
		meshScalings[HumanBodyBones.RightUpperArm] = upperarmMeshScale;
		
		float upperlegMeshScale = modificationCurve.Evaluate(-topBottom + thickness) * 0.3f;
		upperlegMeshScale = limitLow(upperlegMeshScale) + 1f;
		meshScalings[HumanBodyBones.LeftUpperLeg] = upperlegMeshScale;
		meshScalings[HumanBodyBones.RightUpperLeg] = upperlegMeshScale;

		float overallScale = targetAnimator.transform.lossyScale.x;
		
		float hipBoneScale = modificationCurve.Evaluate(-topBottom + thickness) * 0.1f;
		hipBoneScale = limitHigh(hipBoneScale) + 1f;
		boneScalings[HumanBodyBones.Hips] = hipBoneScale;
		
		float spineBoneScale = 1f + modificationCurve.Evaluate(-thickness) * 0.2f;
		boneScalings[HumanBodyBones.Spine] = spineBoneScale * overallScale;

		float chestBoneScale = modificationCurve.Evaluate(topBottom + thickness) * 0.4f;
		chestBoneScale = limitHigh(chestBoneScale) + 1f;
		boneScalings[HumanBodyBones.Chest] = chestBoneScale * overallScale;
		
		float shoulderBoneScale = modificationCurve.Evaluate(topBottom + thickness) * 0.4f;
		shoulderBoneScale = limitHigh(shoulderBoneScale) + 1f;
		boneScalings[HumanBodyBones.LeftShoulder] = shoulderBoneScale * overallScale;
		boneScalings[HumanBodyBones.RightShoulder] = shoulderBoneScale * overallScale;
		
		boneScalings[HumanBodyBones.LeftUpperArm] = overallScale;
		boneScalings[HumanBodyBones.RightUpperArm] = overallScale;
		
		boneScalings[HumanBodyBones.LeftUpperLeg] = overallScale;
		boneScalings[HumanBodyBones.RightUpperLeg] = overallScale;
		
		float handScale = modificationCurve.Evaluate(topBottom) * 0.3f;
		handScale = limitLow(handScale) + 1f;
		boneScalings[HumanBodyBones.LeftHand] = handScale;
		boneScalings[HumanBodyBones.RightHand] = handScale;
		
		float footScale = modificationCurve.Evaluate(-topBottom) * 0.3f;
		footScale = limitLow(footScale) + 1f;
		boneScalings[HumanBodyBones.LeftFoot] = footScale;
		boneScalings[HumanBodyBones.RightFoot] = footScale;
		
		boneScalings[HumanBodyBones.Head] = overallScale;
    }
}
