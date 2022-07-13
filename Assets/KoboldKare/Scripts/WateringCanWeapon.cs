using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WateringCanWeapon : GenericWeapon {
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private FluidStream stream;
    [SerializeField] private GenericReagentContainer container;
    public override void OnFire(GameObject player) {
        base.OnFire(player);
        weaponAnimator.SetBool("Fire", true);
        stream.OnFire(container);
    }

    public override void OnEndFire(GameObject player) {
        base.OnEndFire(player);
        weaponAnimator.SetBool("Fire", false);
        stream.OnEndFire();
    }
}
