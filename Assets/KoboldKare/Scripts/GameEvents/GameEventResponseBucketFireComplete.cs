using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseBucketOnFireComplete : GameEventResponse {
    [SerializeField] private BucketWeapon[] targets;
    public override void Invoke(MonoBehaviour owner){
        base.Invoke(owner);
        foreach (var target in targets) {
            target.OnFireComplete();
        }
    }
}