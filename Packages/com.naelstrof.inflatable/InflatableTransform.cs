using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableTransform : InflatableListener {
        [SerializeField]
        private Transform targetTransform;
        private Vector3 startScale;

        public void SetTransform(Transform newTargetTransform) {
            targetTransform = newTargetTransform;
        }

        public override void OnEnable() {
            startScale = targetTransform.localScale;
        }
        public override void OnSizeChanged(float newSize) {
            if (targetTransform) {
                targetTransform.localScale = startScale * Mathf.Max(newSize, 0.05f);
            }
        }
    }
}
