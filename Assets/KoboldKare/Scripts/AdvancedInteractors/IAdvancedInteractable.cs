using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdvancedInteractable {
    void InteractTo(Vector3 worldPosition, Quaternion worldRotation);
    void OnInteract(Kobold k);
    Transform transform { get; }
    GameObject gameObject { get; }
    void OnEndInteract(Kobold k);
    bool PhysicsGrabbable();
}
