using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;

[CreateAssetMenu(fileName = "SpoilableHandler", menuName = "Data/Spoilable Handler", order = 4)]
public class SpoilableHandler : ScriptableObject {
    private bool running = false;
    public LayerMask safeZoneMask;
    private List<ISpoilable> removeSpoilables = new List<ISpoilable>();
    private List<ISpoilable> spoilables = new List<ISpoilable>();
    public void AddSpoilable(ISpoilable spoilable) {
        spoilables.Add(spoilable);
    }
    public void RemoveSpoilable(ISpoilable spoilable) {
        if (running) {
            removeSpoilables.Add(spoilable);
            return;
        }
        if (spoilables.Contains(spoilable)) {
            spoilables.Remove(spoilable);
        }
    }
    IEnumerator SpoilOverTime(float duration, float multiplier) {
        float startTime = Time.timeSinceLevelLoad;
        while(Time.timeSinceLevelLoad-startTime < duration && DayNightCycle.instance.daylight < -0.9f) {
            for(int i=spoilables.Count-1;i>=0;i--) {
                if (Mathf.Approximately(spoilables[i].spoilIntensity, 1f)) {
                    continue;
                }
                int hitCount = 0;
                foreach( RaycastHit h in Physics.RaycastAll(spoilables[i].transform.position + Vector3.up * 400f, Vector3.down, 400f, safeZoneMask, QueryTriggerInteraction.Collide)) {
                    hitCount++;
                }
                if ( hitCount % 2 == 0 ) {
                    spoilables[i].spoilIntensity = Mathf.MoveTowards(spoilables[i].spoilIntensity,1f,Time.fixedDeltaTime*multiplier);
                } else {
                    spoilables[i].spoilIntensity = Mathf.MoveTowards(spoilables[i].spoilIntensity,0f,Time.fixedDeltaTime*multiplier*2f);
                }
                if (Mathf.Approximately(spoilables[i].spoilIntensity, 1f)) {
                    spoilables[i].onSpoilEvent.Invoke();
                }
            }
            yield return new WaitForFixedUpdate();
        }
        for(int i=spoilables.Count-1;i>=0;i--) {
            spoilables[i].spoilIntensity = 0f;
        }
    }
    //public bool AllPlayersInside() {

    //}
    public void StartSpoilingEvent() {
        foreach(GenericSpoilable spoilable in spoilables) {
            spoilable.DayPassed();
        }
        GameManager.instance.StartCoroutine(SpoilOverTime(6f,0.2f));
    }
}
