using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Photon.Pun;
using UnityEngine;
using UnityEngine.VFX;
using Vilar.AnimationStation;

public class BrainSwapperMachine : UsableMachine, IAnimationStationSet {
    public delegate void BodySwapAction(Kobold a, Kobold b);

    public static event BodySwapAction bodySwapped;
    [SerializeField]
    private Sprite sleepingSprite;
    [SerializeField]
    private List<AnimationStation> stations;

    [SerializeField] private VisualEffect lightning;
    [SerializeField] private AudioPack thunderSound;
    [SerializeField] private AudioPack brainSwapSound;
    private AudioSource brainSwapSoundSource;
    private ReadOnlyCollection<AnimationStation> readOnlyStations;
    void Awake() {
        readOnlyStations = stations.AsReadOnly();
        if (brainSwapSoundSource == null) {
            brainSwapSoundSource = gameObject.AddComponent<AudioSource>();
            brainSwapSoundSource.playOnAwake = false;
            brainSwapSoundSource.maxDistance = 10f;
            brainSwapSoundSource.minDistance = 0.2f;
            brainSwapSoundSource.rolloffMode = AudioRolloffMode.Linear;
            brainSwapSoundSource.spatialBlend = 1f;
            brainSwapSoundSource.loop = false;
        }
    }
    public override Sprite GetSprite(Kobold k) {
        return sleepingSprite;
    }
    public override bool CanUse(Kobold k) {
        if (!constructed) {
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
                // FIXME FISHNET
                // k.photonView.RPC(nameof(CharacterControllerAnimator.BeginAnimationRPC), RpcTarget.All, photonView.ViewID, i);
                break;
            }
        }
        
        // FIXME FISHNET
        // photonView.RPC(nameof(SwapAfterTime), RpcTarget.All);
    }
    // FIXME FISHNET
    // [PunRPC]
    private IEnumerator SwapAfterTime() {
        yield return new WaitForSeconds(4f);
        // FIXME FISHNET
        /*if (!photonView.IsMine) {
            yield break;
        }

        if (stations[0].info.user == null || stations[1].info.user == null) {
            yield break;
        }

        Player aPlayer = null;
        Player bPlayer = null;
        foreach (Player player in PhotonNetwork.PlayerList) {
            if ((Kobold)player.TagObject == stations[0].info.user) {
                aPlayer = player;
            }

            if ((Kobold)player.TagObject == stations[1].info.user) {
                bPlayer = player;
            }
        }

        photonView.RPC(nameof(AssignKobolds), RpcTarget.AllBufferedViaServer, stations[0].info.user.photonView.ViewID,
            stations[1].info.user.photonView.ViewID, bPlayer?.ActorNumber ?? -1, aPlayer?.ActorNumber ?? -1, stations[1].info.user.GetComponent<MoneyHolder>().GetMoney(), stations[0].info.user.GetComponent<MoneyHolder>().GetMoney());
        stations[0].info.user.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All);
        stations[1].info.user.photonView.RPC(nameof(CharacterControllerAnimator.StopAnimationRPC), RpcTarget.All); */
    }

    // FIXME FISHNET
    //[PunRPC]
    public void AssignKobolds(int aViewID, int bViewID, int playerIDA, int playerIDB, float moneyA, float moneyB) {
        // FIXME FISHNET
        /*
        PhotonProfiler.LogReceive(sizeof(int)*4+sizeof(float)*2);
        thunderSound.PlayOneShot(brainSwapSoundSource);
        lightning.Play();
        brainSwapSound.Play(brainSwapSoundSource);
        PhotonView aView = PhotonNetwork.GetPhotonView(aViewID);
        PhotonView bView = PhotonNetwork.GetPhotonView(bViewID);
        if (aView != null) {
            aView.GetComponent<CharacterDescriptor>().SetPlayerControlled(CharacterDescriptor.ControlType.AIPlayer);
        }

        if (bView != null) {
            bView.GetComponent<CharacterDescriptor>().SetPlayerControlled(CharacterDescriptor.ControlType.AIPlayer);
        }
        Player[] playerList = PhotonNetwork.PlayerList;
        Player aPlayer = null;
        Player bPlayer = null;
        foreach (Player player in playerList) {
            if (player.ActorNumber == playerIDA) {
                aPlayer = player;
            }
            if (player.ActorNumber == playerIDB) {
                bPlayer = player;
            }
        }

        if (aView != null) {
            if (Equals(aPlayer, PhotonNetwork.LocalPlayer)) {
                aView.GetComponent<CharacterDescriptor>().SetPlayerControlled(CharacterDescriptor.ControlType.LocalPlayer);
            } else {
                aView.GetComponent<CharacterDescriptor>().SetPlayerControlled(bPlayer == null ? CharacterDescriptor.ControlType.AIPlayer : CharacterDescriptor.ControlType.NetworkedPlayer);
            }

            if (aView.TryGetComponent(out MoneyHolder aMoneyHolder)) {
                aMoneyHolder.SetMoney(moneyA);
            }

            if (aView.TryGetComponent(out Kobold aKobold)) {
                if (aPlayer != null) {
                    aPlayer.TagObject = aKobold;
                }
            }
        }

        if (bView != null) {
            if (Equals(bPlayer, PhotonNetwork.LocalPlayer)) {
                bView.GetComponent<CharacterDescriptor>().SetPlayerControlled(CharacterDescriptor.ControlType.LocalPlayer);
            } else {
                bView.GetComponent<CharacterDescriptor>().SetPlayerControlled(aPlayer == null ? CharacterDescriptor.ControlType.AIPlayer : CharacterDescriptor.ControlType.NetworkedPlayer);
            }
            if (bView.TryGetComponent(out MoneyHolder bMoneyHolder)) {
                bMoneyHolder.SetMoney(moneyB);
            }
            if (bView.TryGetComponent(out Kobold bKobold)) {
                if (bPlayer != null) {
                    bPlayer.TagObject = bKobold;
                }
            }
        }

        if (aView != null && bView != null) {
            bodySwapped?.Invoke(aView.GetComponent<Kobold>(), bView.GetComponent<Kobold>());
        }
        */
    }

    public ReadOnlyCollection<AnimationStation> GetAnimationStations() {
        return readOnlyStations;
    }
}
