using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Photon.Pun;
using KoboldKare;

public class SpoilableHandler : MonoBehaviour {
    [SerializeField]
    private GameEventGeneric midnightEvent;
    private bool running = false;
    [SerializeField]
    private LayerMask safeZoneMask;
    private List<ISpoilable> removeSpoilables = new List<ISpoilable>();
    private List<ISpoilable> spoilables = new List<ISpoilable>();
    private static SpoilableHandler instance;
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
    }
    void Start() {
        midnightEvent.AddListener(OnMidnight);
    }
    void OnDestroy() {
        midnightEvent.RemoveListener(OnMidnight);
    }
    void OnMidnight(object nothing) {
        StartSpoilingEvent();
    }
    public static void AddSpoilable(ISpoilable spoilable) {
        instance.spoilables.Add(spoilable);
    }
    public static void RemoveSpoilable(ISpoilable spoilable) {
        if (instance.running) {
            instance.removeSpoilables.Add(spoilable);
            return;
        }
        if (instance.spoilables.Contains(spoilable)) {
            instance.spoilables.Remove(spoilable);
        }
    }
    IEnumerator SpoilOverTime(float duration, float multiplier) {
        float startTime = Time.timeSinceLevelLoad;
        while(Time.timeSinceLevelLoad-startTime < duration) {
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
    void StartSpoilingEvent() {
        if (DayNightCycle.instance.daylight < -0.9f) {
            GameManager.instance.StartCoroutine(SpoilOverTime(6f,0.2f));
        } else {
            // Fast-forward through the night, just destroy everything outside!
            for(int i=spoilables.Count-1;i>=0;i--) {
                if (spoilables[i] is GenericSpoilable) {
                    if (((GenericSpoilable)spoilables[i]).spawnProtection > 0f) {
                        continue;
                    }
                }
                int hitCount = 0;
                foreach( RaycastHit h in Physics.RaycastAll(spoilables[i].transform.position + Vector3.up * 400f, Vector3.down, 400f, safeZoneMask, QueryTriggerInteraction.Collide)) {
                    hitCount++;
                }
                if ( hitCount % 2 == 0 ) {
                    spoilables[i].spoilIntensity = 1f;
                    spoilables[i].onSpoilEvent.Invoke();
                } else {
                    spoilables[i].spoilIntensity = 0f;
                }
            }
        }
    }
}
