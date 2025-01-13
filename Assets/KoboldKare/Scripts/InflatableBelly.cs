using System.Collections.Generic;
using JigglePhysics;
using UnityEngine;

[System.Serializable]
public class InflatableBelly : Naelstrof.Inflatable.InflatableListener {
    [SerializeField]
    private string blendShapeStartName;
    [SerializeField]
    private string blendShapeContinueName;
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private List<SkinnedMeshRenderer> skinnedMeshRenderers;
    [SerializeField]
    private JiggleSkin skinJiggle;

    private JiggleSkin.JiggleZone skinZone;
    private float skinZoneStartRadius;
    private List<int> blendshapeStartIDs;
    private List<int> blendshapeContinueIDs;
    public override void OnEnable() {
        blendshapeStartIDs = new List<int>();
        blendshapeContinueIDs = new List<int>();
        foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers) {
            int id = renderer.sharedMesh.GetBlendShapeIndex(blendShapeStartName);
            if (id == -1) {
                throw new UnityException($"Cannot find blendshape {blendShapeStartName} on mesh {renderer.sharedMesh}");
            }

            blendshapeStartIDs.Add(id);
            int continueID = renderer.sharedMesh.GetBlendShapeIndex(blendShapeContinueName);
            if (continueID == -1) {
                throw new UnityException(
                    $"Cannot find blendshape {blendShapeContinueName} on mesh {renderer.sharedMesh}");
            }
            blendshapeContinueIDs.Add(continueID);
        }

        if (skinJiggle != null && targetTransform != null) {
            foreach (var jiggleZone in skinJiggle.jiggleZones) {
                if (jiggleZone.GetRootTransform() != targetTransform) continue;
                if (jiggleZone.jiggleSettings is not JiggleSettingsBlend) {
                    throw new UnityException("Belly jiggle settings must be a JiggleSettingsBlend");
                }

                skinZone = jiggleZone;
                skinZoneStartRadius = skinZone.radius;
                jiggleZone.jiggleSettings = JiggleSettingsBlend.Instantiate(jiggleZone.jiggleSettings);
                break;
            }
        }
    }

    public void AddTargetRenderer(SkinnedMeshRenderer renderer) {
        if (skinnedMeshRenderers.Contains(renderer)) {
            return;
        }
        skinnedMeshRenderers.Add(renderer);
        int id = renderer.sharedMesh.GetBlendShapeIndex(blendShapeStartName);
        if (id == -1) {
            Debug.LogWarning( $"Cannot find blendshape {blendShapeStartName} on mesh {renderer.sharedMesh}. This may be intended, though it won't recieve belly changes.");
        }
        blendshapeStartIDs.Add(id);
        int continueID = renderer.sharedMesh.GetBlendShapeIndex(blendShapeContinueName);
        if (continueID == -1) {
            Debug.LogWarning( $"Cannot find blendshape {blendShapeContinueName} on mesh {renderer.sharedMesh}. This may be intended, though it won't recieve belly changes.");
        }
        blendshapeContinueIDs.Add(continueID);
    }

    public void RemoveTargetRenderer(SkinnedMeshRenderer renderer) {
        int index = skinnedMeshRenderers.IndexOf(renderer);
        if (index == -1) {
            return;
        }
        skinnedMeshRenderers.RemoveAt(index);
        blendshapeStartIDs.RemoveAt(index);
        blendshapeContinueIDs.RemoveAt(index);
    }

    public override void OnSizeChanged(float newSize) {
        float startWeight = Mathf.Clamp01(newSize);
        for (int i = 0; i < skinnedMeshRenderers.Count; i++) {
            float continueWeight = Mathf.Max(0f,newSize-1f);
            if (blendshapeStartIDs[i] != -1) {
                skinnedMeshRenderers[i].SetBlendShapeWeight(blendshapeStartIDs[i], startWeight * 100f);
            }
            if (blendshapeContinueIDs[i] != -1) {
                skinnedMeshRenderers[i].SetBlendShapeWeight(blendshapeContinueIDs[i], continueWeight*100f);
            }
        }
        if (skinZone != null) {
            skinZone.radius = skinZoneStartRadius + newSize*skinZoneStartRadius;
            ((JiggleSettingsBlend)skinZone.jiggleSettings).SetNormalizedBlend(Mathf.Clamp01(newSize / 2f));
        }
    }
}
