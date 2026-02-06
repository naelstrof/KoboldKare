using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
public abstract class GenericWeapon : MonoBehaviour {
    [SerializeField]
    private Transform weaponBarrelTransform;
    [SerializeField]
    private Vector3 weaponHoldOffset;

    protected NetworkedEntity networkedEntity;

    protected virtual void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity != null) {
            Debug.Log("Subscribed!!");
            networkedEntity.weaponFireStart += OnFire;
            networkedEntity.weaponFireEnd += OnEndFire;
        } else {
            Debug.Log("Not subscribed!!");
        }
    }

    protected virtual void OnDestroy() {
        if (networkedEntity != null) {
            networkedEntity.weaponFireStart -= OnFire;
            networkedEntity.weaponFireEnd -= OnEndFire;
        }
    }

    public virtual Transform GetWeaponBarrelTransform() {
        return weaponBarrelTransform;
    }
    public virtual Vector3 GetWeaponHoldPosition() {
        return weaponHoldOffset;
    }

    protected abstract void OnEndFire(NetworkedKobold player);
    protected abstract void OnFire(NetworkedKobold player);
}
