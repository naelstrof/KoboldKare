using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Naelstrof.Inflatable {
    public abstract class InflatableListener {
        public abstract void OnEnable();
        public abstract void OnSizeChanged(float newSize);
    }
}
