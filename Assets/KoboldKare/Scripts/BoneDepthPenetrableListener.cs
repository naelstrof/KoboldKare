using System.Collections;
using System.Collections.Generic;
using PenetrationTech;
using UnityEngine;

namespace PenetrationTech {
    [System.Serializable]
    [PenetrableListener(typeof(BoneDepthPenetrableListener), "Bone Depth Penetrable")]
    public class BoneDepthPenetrableListener : PenetrableListener {
        [SerializeField]
        private Transform targetTransform;
        private Penetrable penetrable;
        private Vector3 startScale;

        public override void OnEnable(Penetrable p) {
            base.OnEnable(p);
            penetrable = p;
            startScale = targetTransform.localScale;
        }

        protected override void OnPenetrationDepthChange(float newDepth) {
            base.OnPenetrationDepthChange(newDepth);
            var path = penetrable.GetSplinePath();
            float distT = path.GetDistanceFromTime(t);
            targetTransform.position = penetrable.GetSplinePath().GetPositionFromDistance(distT + newDepth);
        }

        protected override void OnPenetrationGirthRadiusChange(float newGirthRadius) {
            base.OnPenetrationGirthRadiusChange(newGirthRadius);
            targetTransform.localScale = startScale + Vector3.one * newGirthRadius * 20f;
        }

        public override void AssertValid() {
            base.AssertValid();
            if (targetTransform == null) {
                throw new PenetrableListenerValidationException($"targetTransform is null on {this}");
            }
        }

        public override void OnDrawGizmosSelected(Penetrable p) {
#if UNITY_EDITOR
            CatmullSpline path = p.GetSplinePath();
            Vector3 position = path.GetPositionFromT(t);
            Vector3 normal = path.GetVelocityFromT(t).normalized;
            UnityEditor.Handles.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(position, normal, 0.1f);
            UnityEditor.Handles.DrawLine(position, targetTransform.transform.position);
#endif
        }

        public override void NotifyPenetration(Penetrable penetrable, Penetrator penetrator,
            float worldSpaceDistanceToPenisRoot,
            Penetrable.SetClipDistanceAction clipAction) {
            NotifyPenetrationGDO(penetrable, penetrator, worldSpaceDistanceToPenisRoot, clipAction,
                PenData.Depth | PenData.Girth);
        }
    }
}
