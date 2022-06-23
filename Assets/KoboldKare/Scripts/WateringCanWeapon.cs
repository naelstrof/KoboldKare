using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WateringCanWeapon : GenericWeapon {
    private Animator weaponAnimator;
    public override void OnFire(GameObject player) {
        base.OnFire(player);
        weaponAnimator.SetBool("Fire", true);
    }

    public override void OnEndFire(GameObject player) {
        base.OnEndFire(player);
        weaponAnimator.SetBool("Fire", false);
    }
}
