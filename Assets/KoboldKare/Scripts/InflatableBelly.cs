using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Naelstrof.Easing;

[System.Serializable]
public class InflatableBelly : Naelstrof.Inflatable.InflatableListener {
    [SerializeField]
    private string blendShapeStartName;
    [SerializeField]
    private string blendShapeContinueName;
    [SerializeField]
    private List<SkinnedMeshRenderer> skinnedMeshRenderers;
    private List<int> blendshapeStartIDs;
    private List<int> blendshapeContinueIDs;
    public override void OnEnable() {
        blendshapeStartIDs = new List<int>();
        blendshapeContinueIDs = new List<int>();
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers) {
            int id = renderer.sharedMesh.GetBlendShapeIndex(blendShapeStartName);
            if (id == -1) {
                throw new UnityException("Cannot find blendshape " + blendshapeStartIDs + " on mesh " + renderer.sharedMesh);
            }

            blendshapeStartIDs.Add(id);
            int continueID = renderer.sharedMesh.GetBlendShapeIndex(blendShapeContinueName);
            if (continueID == -1) {
                throw new UnityException("Cannot find blendshape " + blendshapeStartIDs + " on mesh " + renderer.sharedMesh);
            }
            blendshapeContinueIDs.Add(continueID);
        }
    }

    public void AddTargetRenderer(SkinnedMeshRenderer renderer) {
        if (skinnedMeshRenderers.Contains(renderer)) {
            return;
        }
        skinnedMeshRenderers.Add(renderer);
        blendshapeStartIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(blendShapeStartName));
        blendshapeContinueIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(blendShapeContinueName));
    }

    public void RemoveTargetRenderer(SkinnedMeshRenderer renderer) {
        int index = skinnedMeshRenderers.IndexOf(renderer);
        if (index == -1) {
            return;
        }
        skinnedMeshRenderers.RemoveAt(index);
        blendshapeStartIDs.RemoveAt(renderer.sharedMesh.GetBlendShapeIndex(blendShapeStartName));
        blendshapeContinueIDs.RemoveAt(renderer.sharedMesh.GetBlendShapeIndex(blendShapeContinueName));
    }

    public override void OnSizeChanged(float newSize) {
        float startWeight = Mathf.Clamp01(newSize);
        for (int i = 0; i < skinnedMeshRenderers.Count; i++) {
            float continueWeight = Mathf.Max(0f,newSize-1f);
            skinnedMeshRenderers[i].SetBlendShapeWeight(blendshapeStartIDs[i], startWeight*100f);
            skinnedMeshRenderers[i].SetBlendShapeWeight(blendshapeContinueIDs[i], continueWeight*100f);
        }
    }
}
