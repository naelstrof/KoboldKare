using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Naelstrof.Mozzarella;
using Photon.Pun;
using SkinnedMeshDecals;
using UnityEngine;
using Vilar.AnimationStation;

public class MilkingTable : GenericUsable, IAnimationStationSet {
    [SerializeField]
    private Sprite milkingSprite;
    [SerializeField]
    private List<AnimationStation> stations;
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    [SerializeField]
    private Material milkSplatMaterial;
    [SerializeField]
    private FluidStream stream;

    private GenericReagentContainer container;

    private WaitForSeconds waitSpurt;
    void Awake() {
        waitSpurt = new WaitForSeconds(1f);
        readOnlyStations = stations.AsReadOnly();
        container = gameObject.AddComponent<GenericReagentContainer>();
        container.type = GenericReagentContainer.ContainerType.Mouth;
        photonView.ObservedComponents.Add(container);
    }
    public override Sprite GetSprite(Kobold k) {
        return milkingSprite;
    }
    public override bool CanUse(Kobold k) {
        if (k.GetEnergy() < 1f) {
            return false;
        }
        foreach (var station in stations) {
            if (station.info.user == null) {
                return true;
            }
        }
        return false;
    }

    public override void LocalUse(Kobold k) {
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null) {
                k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All,
                    photonView.ViewID, i);
                break;
            }
        }
        base.LocalUse(k);
    }
    public override void Use() {
        StopAllCoroutines();
        StartCoroutine(WaitThenMilk());
    }

    [PunRPC]
    private IEnumerator MilkRoutine(int milkKoboldID) {
        PhotonView milkView = PhotonNetwork.GetPhotonView(milkKoboldID);
        if (!milkView.TryGetComponent(out Kobold milkKobold)) {
            yield break;
        }

        // Now do some milk stuff.
        int pulses = 12;
        ReagentContents milkVolume = new ReagentContents();
        float totalVolume = milkKobold.GetGenes().breastSize;
        milkVolume.AddMix(ReagentDatabase.GetReagent("Milk").GetReagent(totalVolume));
        for (int i = 0; i < pulses; i++) {
            foreach (Transform t in milkKobold.GetNipples()) {
                if (MozzarellaPool.instance.TryInstantiate(out Mozzarella mozzarella)) {
                    mozzarella.SetFollowTransform(t);
                    mozzarella.SetVolumeMultiplier(milkVolume.volume * 0.25f);
                    mozzarella.SetLocalForward(Vector3.up);
                    Color color = milkVolume.GetColor();
                    mozzarella.hitCallback += (hit, startPos, dir, length, volume) => {
                        milkSplatMaterial.color = color;
                        PaintDecal.RenderDecalForCollider(hit.collider, milkSplatMaterial,
                            hit.point - hit.normal * 0.1f, Quaternion.LookRotation(hit.normal, Vector3.up)*Quaternion.AngleAxis(UnityEngine.Random.Range(-180f,180f), Vector3.forward),
                            Vector2.one * (volume * 4f), length);
                    };
                }
            }

            if (photonView.IsMine) {
                container.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All,
                    milkVolume.Spill(totalVolume / pulses), photonView.ViewID);
            }

            stream.OnFire(container);
            yield return waitSpurt;
        }
        yield return waitSpurt;
        if (!photonView.IsMine) {
            yield break;
        }
        foreach (var t in stations) {
            if (t.info.user != null && t.info.user.GetEnergy() <= 0) {
                t.info.user.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
            }
        }
    }

    private IEnumerator WaitThenMilk() {
        yield return new WaitForSeconds(8f);
        if (!photonView.IsMine) {
            yield break;
        }
        // Validate that we have two characters with energy that have been animating for 5 seconds
        for (int i = 0; i < stations.Count; i++) {
            if (stations[i].info.user == null || stations[i].info.user.GetEnergy() <= 0) {
                yield break;
            }
        }
        // Consume their energy!
        for (int i = 0; i < stations.Count; i++) {
            if (!stations[i].info.user.TryConsumeEnergy(1)) {
                yield break;
            }
        }

        photonView.RPC(nameof(MilkRoutine), RpcTarget.All, stations[0].info.user.photonView.ViewID);
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
