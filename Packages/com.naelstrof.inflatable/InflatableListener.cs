using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naelstrof.Inflatable {
    [System.Serializable]
    public class InflatableListener {
        public virtual void OnEnable() {
        }

        public virtual void OnSizeChanged(float newSize) {
        }
    }
}
