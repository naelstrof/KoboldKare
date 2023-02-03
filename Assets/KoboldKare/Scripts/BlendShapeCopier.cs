using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeCopier : MonoBehaviour {
    private SkinnedMeshRenderer fromRenderer;
    private SkinnedMeshRenderer toRenderer;
    private struct BlendShapePair {
        public int fromID;
        public int toID;
    }
    private List<BlendShapePair> blendShapePairs;

    void Start() {
        blendShapePairs = new List<BlendShapePair>();
        for (int x = 0; x < fromRenderer.sharedMesh.blendShapeCount; x++) {
            for (int y = 0; y < toRenderer.sharedMesh.blendShapeCount; y++) {
                if (fromRenderer.sharedMesh.GetBlendShapeName(x) == toRenderer.sharedMesh.GetBlendShapeName(y)) {
                    blendShapePairs.Add(new BlendShapePair { fromID = x, toID = y });
                }
            }
        }
    }

    private void LateUpdate() {
        foreach (var blendShapePair in blendShapePairs) {
            toRenderer.SetBlendShapeWeight(blendShapePair.toID,
                fromRenderer.GetBlendShapeWeight(blendShapePair.fromID));
        }
    }
}
