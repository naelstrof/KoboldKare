using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseBucketFireComplete : GameEventResponse
{
    [Serializable]
    private class BucketFireTarget
    {
        [SerializeField] public BucketWeapon Bucket;
    }

    [SerializeField] private BucketFireTarget[] targets;

    public override void Invoke(MonoBehaviour owner){
        base.Invoke(owner);
        foreach (var target in targets) {
            target.Bucket.OnFireComplete();
        }
    }
}