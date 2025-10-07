using System;
using UnityEngine;
using UnityEngine.VFX;

[System.Serializable]
public class GameEventResponseParticleSystemPlay : GameEventResponse {
    [SerializeField] private VisualEffect[] targets;

    public override void Invoke(MonoBehaviour owner){
        base.Invoke(owner);
        foreach (var target in targets) {
            target.Play();
        }
    }
}