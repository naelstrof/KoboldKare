using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class Sleeper : MonoBehaviour {
    public GameEvent startSleep;
    public GameEvent sleep;
    public void TrySleep() {
        foreach(var playerpair in NetworkManager.instance.playerList) {
            if (playerpair.Value != null) {
                if (Vector3.Distance(playerpair.Value.transform.position,transform.position)<10f) {
                    StopAllCoroutines();
                    StartCoroutine(SleepRoutine());
                }
            }
        }
    }
    private IEnumerator SleepRoutine() {
        startSleep.Raise();
        yield return new WaitForSeconds(0.5f);
        sleep.Raise();
    }
}
