using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Naelstrof.Easing;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableBlendShape : InflatableListener {
        [SerializeField]
        private string blendShapeName;
        [SerializeField]
        private List<SkinnedMeshRenderer> skinnedMeshRenderers;
        [SerializeField]
        private bool clamped;
        private List<int> blendshapeIDs;

        private void AddBlendShape(SkinnedMeshRenderer renderer, bool adding) {
            var index = renderer.sharedMesh.GetBlendShapeIndex(blendShapeName);
            if(index == -1 || (adding && skinnedMeshRenderers.Contains(renderer))) {
                return;
            }
            if(adding) {
                skinnedMeshRenderers.Add(renderer);
            }
            blendshapeIDs.Add(index);
        }

        public override void OnEnable() {
            blendshapeIDs = new List<int>();
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers) {
                AddBlendShape(renderer, false);
            }
        }

        public void AddTargetRenderer(SkinnedMeshRenderer renderer) {
            if (skinnedMeshRenderers.Contains(renderer)) {
                return;
            }
            AddBlendShape(renderer, true);
        }

        public void RemoveTargetRenderer(SkinnedMeshRenderer renderer) {
            int index = skinnedMeshRenderers.IndexOf(renderer);
            if (index == -1) {
                return;
            }
            skinnedMeshRenderers.RemoveAt(index);
            blendshapeIDs.RemoveAt(index);
        }

        public override void OnSizeChanged(float newSize) {
            for (int i = 0; i < skinnedMeshRenderers.Count; i++) {
                if (blendshapeIDs[i] != -1) {
                    skinnedMeshRenderers[i].SetBlendShapeWeight(blendshapeIDs[i], clamped?Mathf.Clamp01(newSize)*100f : newSize*100f);
                }
            }
        }
    }
}
