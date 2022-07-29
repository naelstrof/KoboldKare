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
            return k.GetEnergy() > 0 && k.metabolizedContents.volume > 0f;
        }
        
        return k.GetEnergy() > 0;
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

    public override void Use() {
        base.Use();
        StopAllCoroutines();
        StartCoroutine(CrusherRoutine());
    }

    private IEnumerator CrusherRoutine() {
        if (stations[0].info.user != null && stations[0].info.user.photonView.IsMine) {
            photonView.RequestOwnership();
        }
        yield return new WaitForSeconds(6f);
        foreach (var t in stations) {
            if (t.info.user == null || t.info.user.GetEnergy() <= 0) {
                yield break;
            }
        }
        foreach (var t in stations) {
            t.info.user.TryConsumeEnergy(1);
        }

        Kobold pressedKobold = stations[0].info.user;
        if (!pressedKobold.photonView.IsMine) {
            pressedKobold.photonView.RequestOwnership();
        }

        ReagentContents spilled = pressedKobold.metabolizedContents.Spill(pressedKobold.metabolizedContents.volume);
        pressedKobold.InverseProcessReagents(spilled);
        container.AddMix(spilled, GenericReagentContainer.InjectType.Inject);
        container.AddMix(pressedKobold.bellyContainer.Spill(pressedKobold.bellyContainer.volume), GenericReagentContainer.InjectType.Inject);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
