using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILODConsumer {
    void SetLOD(int lod);
    Vector3 GetPosition();
    bool Valid();
}
