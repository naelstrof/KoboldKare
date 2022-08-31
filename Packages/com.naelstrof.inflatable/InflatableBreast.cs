using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JigglePhysics;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using Naelstrof.Easing;
using UnityEngine.Serialization;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableBreast : InflatableListener {
        [SerializeField]
        private Transform baseBreastTransform;
        [FormerlySerializedAs("targetTransform")] [SerializeField]
        private Transform breastTransform;
        [SerializeField]
        private string flatShape;
        [SerializeField]
        private string biggerShape;
        [SerializeField]
        private List<SkinnedMeshRenderer> skinnedMeshRenderers;
        [SerializeField]
        private JiggleRigBuilder rigBuilder;
        [SerializeField]
        private JiggleSkin skinJiggle;
        [SerializeField]
        private Animator breastAnimator;

        private JiggleSkin.JiggleZone skinZone;
        private float skinZoneStartRadius;
        
        private Vector3 startScale;
        private List<int> flatShapeIDs;
        private List<int> biggerShapeIDs;
        private JiggleRigBuilder.JiggleRig targetRig;
        private static readonly int BreastSize = Animator.StringToHash("BreastSize");

        public override void OnEnable() {
            flatShapeIDs = new List<int>();
            biggerShapeIDs = new List<int>();
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers) {
                flatShapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(flatShape));
                biggerShapeIDs.Add(renderer.sharedMesh.GetBlendShapeIndex(biggerShape));
            }
            startScale = baseBreastTransform.localScale;
            foreach (var jiggleRig in rigBuilder.jiggleRigs) {
                if (breastTransform.IsChildOf(jiggleRig.rootTransform)) {
                    if (jiggleRig.jiggleSettings is not JiggleSettingsBlend) {
                        throw new UnityException("Breast jiggle settings must be a JiggleSettingsBlend");
                    }
                    targetRig = jiggleRig;
                    targetRig.jiggleSettings = JiggleSettingsBlend.Instantiate(targetRig.jiggleSettings);
                    break;
                }
            }

            foreach (var jiggleZone in skinJiggle.jiggleZones) {
                if (jiggleZone.target == breastTransform) {
                    if (jiggleZone.jiggleSettings is not JiggleSettingsBlend) {
                        throw new UnityException("Breast jiggle settings must be a JiggleSettingsBlend");
                    }
                    skinZone = jiggleZone;
                    skinZoneStartRadius = skinZone.radius;
                    jiggleZone.jiggleSettings = JiggleSettingsBlend.Instantiate(jiggleZone.jiggleSettings);
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
            ((JiggleSettingsBlend)skinZone.jiggleSettings).normalizedBlend = Mathf.Clamp01(newSize / 3f);
            skinZone.radius = skinZoneStartRadius + newSize*skinZoneStartRadius;
            baseBreastTransform.localScale = startScale + Vector3.one*Mathf.Max(newSize-2f, 0f);
            breastAnimator.SetFloat(BreastSize, newSize);
        }
    }
}
