using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;
using System.IO;
using System.Threading.Tasks;
using SimpleJSON;

public class Sleeper : MonoBehaviour {
    public GameEventGeneric startSleep;
    public GameEventGeneric sleep;
    public Sprite sleepSprite;
    private NetworkedEntity networkedEntity;

    private void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(sleepSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold k) {
        // FIXME FISHNET
        /*bool canSleep = true;
        foreach(var player in PhotonNetwork.PlayerList) {
            if (player.TagObject != null) {
                if (Vector3.Distance((player.TagObject as Kobold).transform.position,transform.position)>10f) {
                    canSleep = false;
                    break;
                }
            }
        }*/
        return true;
    }
    private void OnUse(NetworkedKobold k) {
        StopAllCoroutines();
        StartCoroutine(SleepRoutine());
    }
    private IEnumerator SleepRoutine() {
        startSleep.Raise(null);
        yield return new WaitForSeconds(0.5f);
        sleep.Raise(null);
        //DayNightCycle.StaticSleep();
    }
}
