using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
public class GenericWeapon : MonoBehaviourPun {
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
    public void OnEndFire(Kobold player) {
        photonView.RPC(nameof(OnEndFireRPC), RpcTarget.All, player.photonView.ViewID);
    }
    public void OnFire(Kobold player) {
        photonView.RPC(nameof(OnFireRPC), RpcTarget.All, player.photonView.ViewID);
    }

    [PunRPC]
    protected virtual void OnFireRPC(int playerID) {
        PhotonProfiler.LogReceive(sizeof(int));
    }
    [PunRPC]
    protected virtual void OnEndFireRPC(int playerID) {
        PhotonProfiler.LogReceive(sizeof(int));
    }
}
