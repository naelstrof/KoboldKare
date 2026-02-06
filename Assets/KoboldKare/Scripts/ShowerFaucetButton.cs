using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ShowerFaucetButton : UsableMachine {
    [SerializeField] private FluidStream stream;
    [SerializeField] private Sprite useSprite;
    [SerializeField] private GenericReagentContainer container;
    private NetworkedEntity networkedEntity;

    protected override void Start() {
        base.Start();
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(useSprite);
            networkedEntity.used += OnUse;
            networkedEntity.useRequested += OnUseRequested;
        }
    }

    private bool firing = false;
    private bool OnUseRequested(NetworkedKobold k) {
        return constructed;
    }

    private void OnUse(NetworkedKobold by) {
        firing = !firing;
        if (firing) {
            stream.OnFire(by);
        } else {
            stream.OnEndFire();
        }
    }
}
