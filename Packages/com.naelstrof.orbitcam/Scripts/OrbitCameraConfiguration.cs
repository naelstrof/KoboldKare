using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class OrbitCameraConfiguration {
    public abstract OrbitCameraData GetData(Camera cam);
    public virtual LayerMask GetCullingMask() => ~0;
}
