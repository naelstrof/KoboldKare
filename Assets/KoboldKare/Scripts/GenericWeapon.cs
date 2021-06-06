using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class PlayerEvent : UnityEvent<GameObject> {}
public class GenericWeapon : MonoBehaviour, IWeapon {
    public Transform weaponBarrelTransform;
    public Vector3 weaponHoldOffset;
    public PlayerEvent onFire;
    public PlayerEvent onEndFire;

    public Transform GetWeaponBarrelTransform() {
        return weaponBarrelTransform;
    }
    public Vector3 GetWeaponHoldPosition() {
        return weaponHoldOffset;
    }
    public void OnEndFire(GameObject player) {
        onEndFire.Invoke(player);
    }
    public void OnFire(GameObject player) {
        onFire.Invoke(player);
    }
}
