using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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
    public void OnEndFire(NetworkedKobold player) {
        // FIXME FISHNET
        //photonView.RPC(nameof(OnEndFireRPC), RpcTarget.All, player.photonView.ViewID);
    }
    public void OnFire(NetworkedKobold player) {
        // FIXME FISHNET
        //photonView.RPC(nameof(OnFireRPC), RpcTarget.All, player.photonView.ViewID);
    }

    // FIXME FISHNET
    //[PunRPC]
    protected virtual void OnFireRPC(int playerID) {
    }
    // FIXME FISHNET
    //[PunRPC]
    protected virtual void OnEndFireRPC(int playerID) {
    }
}
