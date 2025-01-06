using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using Naelstrof.Easing;
using Naelstrof.Inflatable;
using UnityEngine;

[System.Serializable]
public class BonerInflatable : InflatableListener {
    [SerializeField]
    private string limpBlendshape;
    [SerializeField]
    private string flaccidBlendshape;
    
    [SerializeField]
    private List<SkinnedMeshRenderer> targetRenderers;
    
    
    [SerializeField]
    private JiggleRigBuilder rigBuilder;

    private JiggleSettingsBlend targetBlend;
    
    private List<int> limpBlendshapeIDs;
    private List<int> flaccidBlendshapeIDs;
    public override void OnEnable() {
        limpBlendshapeIDs = new List<int>();
        flaccidBlendshapeIDs = new List<int>();
        foreach (SkinnedMeshRenderer renderer in targetRenderers) {
            limpBlendshapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(limpBlendshape));
            flaccidBlendshapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(flaccidBlendshape));
        }

        if (rigBuilder.jiggleRigs == null || rigBuilder.jiggleRigs.Count <= 0) {
            throw new UnityException("Jiggle rig builder must have at least one JiggleRig on it");
        }

        foreach (var jiggleRig in rigBuilder.jiggleRigs) {
            if (!(jiggleRig.jiggleSettings is JiggleSettingsBlend)) {
                throw new UnityException("Jiggle rig needs a blended jigglesettings");
            }
            targetBlend = JiggleSettingsBlend.Instantiate((JiggleSettingsBlend)jiggleRig.jiggleSettings);
            jiggleRig.jiggleSettings = targetBlend;
            break;
        }
    }

    public override void OnSizeChanged(float newSize) {
        float size = Mathf.Clamp01(newSize);
        float limpTriggerAmount = Easing.Cubic.InOut(1f-Mathf.Abs((size - 0.5f) * 2f));
        float flaccidTriggerAmount = Easing.Cubic.In(1f - size);
        for(int i=0;i<targetRenderers.Count;i++) {
            targetRenderers[i].SetBlendShapeWeight(limpBlendshapeIDs[i], limpTriggerAmount * 100f);
            targetRenderers[i].SetBlendShapeWeight(flaccidBlendshapeIDs[i], flaccidTriggerAmount * 100f);
        }
        targetBlend.SetNormalizedBlend(Mathf.Clamp01(Mathf.SmoothStep(0f,1f,size)));
    }
}
