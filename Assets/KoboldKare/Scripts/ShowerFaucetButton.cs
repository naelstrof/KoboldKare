using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ShowerFaucetButton : GenericUsable, ISavable, IPunObservable {
    [SerializeField] private FluidStream stream;
    [SerializeField] private Sprite useSprite;
    [SerializeField] private GenericReagentContainer container;
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    private bool firing = false;
    public override void Use() {
        firing = !firing;
        if (firing) {
            stream.OnFire(container);
        } else {
            stream.OnEndFire();
        }
    }
}
