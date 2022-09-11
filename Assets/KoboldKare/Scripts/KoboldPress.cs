using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class KoboldPress : GenericUsable, IAnimationStationSet {
    [SerializeField]
    private List<AnimationStation> stations;
    [SerializeField]
    private Sprite useSprite;
    [SerializeField]
    private FluidStream stream;
    
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    private GenericReagentContainer container;
    void Start() {
        readOnlyStations = stations.AsReadOnly();
        
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        photonView.ObservedComponents.Add(container);
        container.OnChange.AddListener(OnReagentContentsChanged);
    }
    private void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        stream.OnFire(container);
    }

    public override Sprite GetSprite(Kobold k) {
        return useSprite;
    }

    public override bool CanUse(Kobold k) {
        int slot = -1;
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null) {
                slot = i;
                break;
            }
        }

        if (slot == -1) {
            return false;
        }

        if (slot == 0) {
            return k.GetEnergy() >= 1f && k.bellyContainer.volume > 0f;
        }
        
        return k.GetEnergy() >= 1f;
    }

    public override void LocalUse(Kobold k) {
        base.LocalUse(k);
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null) {
                k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,photonView.ViewID, i);
                break;
            }
        }

    }

    [PunRPC]
    public override void Use() {
        base.Use();
        StopAllCoroutines();
        StartCoroutine(CrusherRoutine());
    }

    private IEnumerator CrusherRoutine() {
        yield return new WaitForSeconds(6f);
        if (!photonView.IsMine) {
            yield break;
        }
        foreach (var t in stations) {
            if (t.info.user == null || t.info.user.GetEnergy() <= 0) {
                yield break;
            }
        }
        foreach (var t in stations) {
            //t.info.user.photonView.RPC(nameof(Kobold.ConsumeEnergyRPC), RpcTarget.All, (byte)1);
            t.info.user.TryConsumeEnergy(1);
        }
        Kobold pressedKobold = stations[0].info.user;
        pressedKobold.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.Others,
            pressedKobold.bellyContainer.volume);
        ReagentContents spilled = pressedKobold.bellyContainer.Spill(pressedKobold.bellyContainer.volume);
        
        container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, spilled,
            pressedKobold.photonView.ViewID);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
