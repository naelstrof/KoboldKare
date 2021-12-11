using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayButton : GenericUsable {
    [SerializeField]
    private Sprite onSprite;
    [SerializeField]
    private Sprite offSprite;
    public FluidOutputMozzarellaSquirt squirter;
    public AudioSource aud;
    public override Sprite GetSprite(Kobold k) {
        return on ? onSprite : offSprite;
    }
    private bool on {
        get {
            return (usedCount % 2) != 0;
        }
    }
    public override void Use(Kobold k) {
        base.Use(k);
        if (on) {
            squirter.Fire();
            aud.Play();
        } else {
            squirter.StopFiring();
            aud.Stop();
        }
    }
}
