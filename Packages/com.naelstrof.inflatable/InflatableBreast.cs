using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Naelstrof.Easing;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableBreast : InflatableListener {
        [SerializeField]
        private Transform targetTransform;
        [SerializeField]
        private string flatShape;
        [SerializeField]
        private string biggerShape;
        [SerializeField]
        private List<SkinnedMeshRenderer> skinnedMeshRenderers;
        [SerializeField]
        private JiggleRigBuilder rigBuilder;
        
        private Vector3 startScale;
        private List<int> flatShapeIDs;
        private List<int> biggerShapeIDs;
        private JiggleRigBuilder.JiggleRig targetRig;
        public override void OnEnable() {
            flatShapeIDs = new List<int>();
            biggerShapeIDs = new List<int>();
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers) {
                flatShapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(flatShape));
                biggerShapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(biggerShape));
            }
            this.targetTransform = targetTransform;
            startScale = targetTransform.localScale;
            foreach (var jiggleRig in rigBuilder.jiggleRigs) {
                if (targetTransform.IsChildOf(jiggleRig.rootTransform)) {
                    if (!jiggleRig.jiggleSettings is JiggleSettingsBlend) {
                        throw new UnityException("Breast jiggle settings must be a JiggleSettingsBlend");
                    }
                    targetRig = jiggleRig;
                    break;
                }
            }
        }

        public void AddTargetRenderer(SkinnedMeshRenderer renderer) {
            skinnedMeshRenderers.Add(renderer);
            flatShapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(flatShape));
            biggerShapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(biggerShape));
        }

        public void RemoveTargetRenderer(SkinnedMeshRenderer renderer) {
            int index = skinnedMeshRenderers.IndexOf(renderer);
            skinnedMeshRenderers.RemoveAt(index);
            flatShapeIDs.RemoveAt(index);
            biggerShapeIDs.RemoveAt(index);
        }

        public override void OnSizeChanged(float newSize) {
            float flatChestSize = Easing.Easing.Cubic.Out(Mathf.Clamp01(1f - newSize));
            float biggerChestSize = Easing.Easing.Cubic.Out(Mathf.Clamp01(newSize-1f));
            for (int i = 0; i < skinnedMeshRenderers.Count; i++) {
                skinnedMeshRenderers[i].SetBlendShapeWeight(flatShapeIDs[i], flatChestSize * 100f);
                skinnedMeshRenderers[i].SetBlendShapeWeight(biggerShapeIDs[i], biggerChestSize * 100f);
            }
            ((JiggleSettingsBlend)targetRig.jiggleSettings).normalizedBlend = Mathf.Clamp01(newSize / 3f);
            targetTransform.localScale = startScale + Vector3.one*Mathf.Max(newSize-2f, 0f);
        }
    }
}
