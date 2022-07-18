using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class BreedingMount : GenericUsable {
    [SerializeField] private Sprite useSprite;
    [SerializeField] private AnimationStation station;
    [SerializeField] private FluidStream stream;
    private GenericReagentContainer container;
    private void Awake() {
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        photonView.ObservedComponents.Add(container);
        container.OnChange.AddListener(OnReagentContentsChanged);
    }

    private void OnReagentContentsChanged(GenericReagentContainer.InjectType injectType) {
        StopCoroutine(nameof(WaitThenFire));
        StartCoroutine(nameof(WaitThenFire));
    }

    IEnumerator WaitThenFire() {
        while (container.volume < 10f && station.info.user != null) {
            yield return null;
        }
        stream.OnFire(container);
    }

    public override bool CanUse(Kobold k) {
        return k.GetEnergy() > 0 && station.info.user == null;
    }
    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }
    public override void LocalUse(Kobold k) {
        k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, 0);
    }
    
}
