using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;

public class Sleeper : MonoBehaviour {
    public GameEvent startSleep;
    public GameEvent sleep;
    public void TrySleep() {
        bool canSleep = true;
        foreach(var player in PhotonNetwork.PlayerList) {
            if (player.TagObject != null) {
                if (Vector3.Distance((player.TagObject as Kobold).transform.position,transform.position)>10f) {
                    canSleep = false;
                    break;
                }
            }
        }
        if (canSleep) {
            StopAllCoroutines();
            StartCoroutine(SleepRoutine());
        }
    }
    private IEnumerator SleepRoutine() {
        startSleep.Raise();
        yield return new WaitForSeconds(0.5f);
        sleep.Raise();
    }
}
