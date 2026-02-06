using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class WateringCanWeapon : GenericWeapon {
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private FluidStream stream;
    [SerializeField] private GenericReagentContainer container;
    private static readonly int Fire = Animator.StringToHash("Fire");
    // FIXME FISHNET
    //[PunRPC]
    protected override void OnFire(NetworkedKobold player) {
        weaponAnimator.SetBool(Fire, true);
        if (networkedEntity) {
            stream.OnFire(networkedEntity);
        }
    }

    // FIXME FISHNET
    //[PunRPC]

    protected override void OnEndFire(NetworkedKobold player) {
        weaponAnimator.SetBool(Fire, false);
        stream.OnEndFire();
    }
}
