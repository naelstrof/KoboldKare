using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BucketWeapon : GenericWeapon {
    [SerializeField]
    private PhotonGameObjectReference bucketSplashProjectile;
    [SerializeField]
    private GenericReagentContainer container;
    [SerializeField]
    private Animator bucketAnimator;

    private static readonly int Fire = Animator.StringToHash("Fire");

    public override void OnFire(GameObject player) {
        base.OnFire(player);
        bucketAnimator.SetTrigger(Fire);
    }

    public void OnFireComplete() {
        Vector3 velocity = GetWeaponBarrelTransform().forward * 5f;
        PhotonNetwork.Instantiate(bucketSplashProjectile.photonName, GetWeaponBarrelTransform().position,
            GetWeaponBarrelTransform().rotation, 0, new object[] { container.Spill(10f), velocity});
    }
}
