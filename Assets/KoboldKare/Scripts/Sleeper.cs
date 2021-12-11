using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using Photon.Pun;

public class Sleeper : GenericUsable {
    public GameEventGeneric startSleep;
    public GameEventGeneric sleep;
    public Sprite sleepSprite;
    public override Sprite GetSprite(Kobold k) {
        return sleepSprite;
    }
    public override bool CanUse(Kobold k) {
        bool canSleep = true;
        foreach(var player in PhotonNetwork.PlayerList) {
            if (player.TagObject != null) {
                if (Vector3.Distance((player.TagObject as Kobold).transform.position,transform.position)>10f) {
                    canSleep = false;
                    break;
                }
            }
        }
        return canSleep;
    }
    public override void Use(Kobold k) {
        base.Use(k);
        StopAllCoroutines();
        StartCoroutine(SleepRoutine());
    }
    private IEnumerator SleepRoutine() {
        startSleep.Raise(null);
        yield return new WaitForSeconds(0.5f);
        sleep.Raise(null);
        DayNightCycle.instance.Sleep();
    }
}
