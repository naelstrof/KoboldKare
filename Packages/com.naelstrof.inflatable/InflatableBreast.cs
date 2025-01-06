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
        [SerializeField,Tooltip("The breast transform that will get scaled to be larger.")]
        private Transform baseBreastTransform;
        [FormerlySerializedAs("breastTransform")] [SerializeField] [Tooltip("A transform that is a child of the JiggleRig, in order to adjust the jiggle settings blend when it gets bigger/smaller")]
        private Transform jiggleBoneBreastTransform;
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
        [SerializeField]
        private Vector3 breastForward = Vector3.up;

        private JiggleSkin.JiggleZone skinZone;
        private float skinZoneStartRadius;
        
        private Vector3 startScale;
        private List<int> flatShapeIDs;
        private List<int> biggerShapeIDs;
        private JiggleRigBuilder.JiggleRig targetRig;
        private static readonly int BreastSize = Animator.StringToHash("BreastSize");

        private void AddBlendShape(SkinnedMeshRenderer renderer, bool adding) {
            var flatIndex = renderer.sharedMesh.GetBlendShapeIndex(flatShape);
            var biggerIndex = renderer.sharedMesh.GetBlendShapeIndex(biggerShape);
            if (flatIndex == -1 || biggerIndex == -1 || (adding && skinnedMeshRenderers.Contains(renderer))) {
                return;
            }
            if(adding) {
                skinnedMeshRenderers.Add(renderer);
            }
            flatShapeIDs.Add(flatIndex);
            biggerShapeIDs.Add(biggerIndex);
        }

        public override void OnEnable() {
            flatShapeIDs = new List<int>();
            biggerShapeIDs = new List<int>();
            foreach (SkinnedMeshRenderer renderer in skinnedMeshRenderers) {
                AddBlendShape(renderer, false);
            }
            startScale = baseBreastTransform.localScale;
            if (rigBuilder != null && jiggleBoneBreastTransform != null) {
                foreach (var jiggleRig in rigBuilder.jiggleRigs) {
                    if (!jiggleBoneBreastTransform.IsChildOf(jiggleRig.GetRootTransform())) continue;
                    if (jiggleRig.jiggleSettings is not JiggleSettingsBlend) {
                        throw new UnityException("Breast jiggle settings must be a JiggleSettingsBlend");
                    }

                    targetRig = jiggleRig;
                    targetRig.jiggleSettings = JiggleSettingsBlend.Instantiate(targetRig.jiggleSettings);
                    break;
                }
            }

            if (skinZone != null && jiggleBoneBreastTransform != null) {
                foreach (var jiggleZone in skinJiggle.jiggleZones) {
                    if (jiggleZone.GetRootTransform() != jiggleBoneBreastTransform) continue;
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
            if(skinnedMeshRenderers.Contains(renderer)) return;
            AddBlendShape(renderer, true);
        }

        public void RemoveTargetRenderer(SkinnedMeshRenderer renderer) {
            int index = skinnedMeshRenderers.IndexOf(renderer);
            if(index == -1) {
                return;
            }
            skinnedMeshRenderers.RemoveAt(index);
            flatShapeIDs.RemoveAt(index);
            biggerShapeIDs.RemoveAt(index);
        }

        public override void OnSizeChanged(float newSize) {
            float flatChestSize = Easing.Easing.Cubic.Out(Mathf.Clamp01(1f - newSize));
            float biggerChestSize = Easing.Easing.Cubic.Out(Mathf.Clamp01(newSize-1f));
            for (int i = 0; i < skinnedMeshRenderers.Count; i++) {
                if (flatShapeIDs[i] != -1) {
                    skinnedMeshRenderers[i].SetBlendShapeWeight(flatShapeIDs[i], flatChestSize * 100f);
                }

                if (biggerShapeIDs[i] != -1) {
                    skinnedMeshRenderers[i].SetBlendShapeWeight(biggerShapeIDs[i], biggerChestSize * 100f);
                }
            }

            if (rigBuilder != null) {
                ((JiggleSettingsBlend)targetRig.jiggleSettings).SetNormalizedBlend(Mathf.Clamp01(newSize / 3f));
            }

            if (skinZone != null) {
                ((JiggleSettingsBlend)skinZone.jiggleSettings).SetNormalizedBlend(Mathf.Clamp01(newSize / 3f));
                skinZone.radius = skinZoneStartRadius + newSize*skinZoneStartRadius;
            }

            baseBreastTransform.localScale = startScale + (Vector3.one-breastForward*0.25f)*Mathf.Max(newSize-2f, 0f);
            breastAnimator.SetFloat(BreastSize, newSize);
        }
    }
}
