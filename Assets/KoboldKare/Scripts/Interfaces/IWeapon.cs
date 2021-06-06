using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IWeapon {
    void OnFire(GameObject player);
    void OnEndFire(GameObject player);
    Transform GetWeaponBarrelTransform();
    Vector3 GetWeaponHoldPosition();
}
