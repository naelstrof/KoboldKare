using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    protected override void Start() {
        base.Start();
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
        if (!constructed) {
            return false;
        }
        if (stations[0].info.user == null) {
            return k.bellyContainer.volume > 0f;
        }
        return false;
    }

    public override void LocalUse(Kobold k) {
        base.LocalUse(k);       
        if (stations[0].info.user == null) {
            k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,photonView.ViewID, 0);
        }
    }

    [PunRPC]
    public override void Use() {
        base.Use();
        StopAllCoroutines();
        StartCoroutine(CrusherRoutine());
    }

    private IEnumerator CrusherRoutine() {
        // added by Godeken
        yield return new WaitForSeconds(1f);
        
        anim.SetTrigger("BeingPressed");

        yield return new WaitForSeconds(6f);
        
        if (!photonView.IsMine) {
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
            pressedKobold.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
