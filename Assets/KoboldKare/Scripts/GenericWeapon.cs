using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class GenericWeapon : MonoBehaviour {
    [SerializeField]
    private Transform weaponBarrelTransform;
    [SerializeField]
    private Vector3 weaponHoldOffset;
    public virtual Transform GetWeaponBarrelTransform() {
        return weaponBarrelTransform;
    }
    public virtual Vector3 GetWeaponHoldPosition() {
        return weaponHoldOffset;
    }
    public virtual void OnEndFire(GameObject player) {
    }
    public virtual void OnFire(GameObject player) {
    }
}
