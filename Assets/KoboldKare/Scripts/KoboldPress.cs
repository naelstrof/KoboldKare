using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FishNet.Object;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;
using Vilar.AnimationStation;

public class KoboldPress : UsableMachine, IAnimationStationSet {
    [SerializeField]
    private List<AnimationStation> stations;
    [SerializeField]
    private Sprite useSprite;
    [SerializeField]
    private FluidStream stream;

    // added by Godeken
    [SerializeField] private Animator anim;

    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    private GenericReagentContainer container;

    private NetworkedEntity networkedEntity;
    
    protected override void Start() {
        base.Start();
        readOnlyStations = stations.AsReadOnly();
        
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        // FIXME FISHNET
        //photonView.ObservedComponents.Add(container);
        container.OnChange += OnReagentContentsChanged;
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(useSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }
    
    private void OnReagentContentsChanged(ReagentContents contents, GenericReagentContainer.InjectType injectType) {
        stream.OnFire(container);
    }

    private bool OnUseRequested(NetworkedKobold k) {
        if (!constructed) {
            return false;
        }

        if (!k.TryGetKobold(out Kobold kobold)) {
            return false;
        }
        if (stations[0].info.user == null) {
            return kobold.bellyContainer.volume > 0f;
        }
        return false;
    }

    private void OnUse(NetworkedKobold k) {
        if (stations[0].info.user == null) {
            k.BeginAnimation(GetComponentInParent<NetworkObject>(), 0);
        }
        StopAllCoroutines();
        StartCoroutine(CrusherRoutine());
    }

    private IEnumerator CrusherRoutine() {
        // added by Godeken
        yield return new WaitForSeconds(1f);
        
        anim.SetTrigger("BeingPressed");

        yield return new WaitForSeconds(6f);
        
        // FIXME FISHNET
        /*if (!photonView.IsMine) {
            yield break;
        }

        Kobold pressedKobold = stations[0].info.user;

        if (pressedKobold == null) {
            yield break;
        }
        pressedKobold.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.Others,
            pressedKobold.bellyContainer.volume);
        ReagentContents spilled = pressedKobold.bellyContainer.Spill(pressedKobold.bellyContainer.volume);
        
        BitBuffer buffer = new BitBuffer(4);
        buffer.AddReagentContents(spilled);
        container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, buffer,
            pressedKobold.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);*/
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
