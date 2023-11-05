using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableTransformClamped : InflatableListener {
        [SerializeField]
        private Transform targetTransform;
        [SerializeField] private float minScale = 0.05f;
        [SerializeField] private float maxScale = float.MaxValue;
        private Vector3 startScale;

        public void SetTransform(Transform newTargetTransform) {
            targetTransform = newTargetTransform;
        }

        public override void OnEnable() {
            startScale = targetTransform.localScale;
        }
        public override void OnSizeChanged(float newSize) {
            targetTransform.localScale = startScale*Mathf.Clamp(newSize, minScale, maxScale);
        }
    }
}
