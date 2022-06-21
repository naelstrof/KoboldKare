using System;
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

    [SerializeField]
    private Rigidbody body;

    private static readonly int Fire = Animator.StringToHash("Fire");

    public override void OnFire(GameObject player) {
        base.OnFire(player);
        bucketAnimator.SetTrigger(Fire);
    }

    public void OnFireComplete() {
        if (container.volume > 0.1f) {
            Vector3 velocity = GetWeaponBarrelTransform().forward * 10f + body.velocity * 0.5f;
            GameObject obj = PhotonNetwork.Instantiate(bucketSplashProjectile.photonName,
                GetWeaponBarrelTransform().position,
                GetWeaponBarrelTransform().rotation, 0, new object[] { container.Spill(10f), velocity });
            obj.GetComponent<Projectile>().LaunchFrom(body);
        }
    }

    private void OnValidate() {
        bucketSplashProjectile.OnValidate();
    }
}
