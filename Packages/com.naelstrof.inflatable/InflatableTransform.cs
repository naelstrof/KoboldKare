using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableTransform : InflatableListener {
        [SerializeField]
        private Transform targetTransform;
        
        private Vector3 startScale;
        public InflatableTransform(Transform targetTransform) {
            this.targetTransform = targetTransform;
        }
        public override void OnEnable() {
            startScale = targetTransform.localScale;
        }
        public override void OnSizeChanged(float newSize) {
            targetTransform.localScale = startScale*Mathf.Max(newSize,0.025f);
        }
    }
}
