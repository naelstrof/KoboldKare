using System;
using UnityEngine;
using UnityEngine.VFX;

[System.Serializable]
public class GameEventResponseParticleSystemSendEvent : GameEventResponse {
    
    [System.Serializable]
    private struct VFXEventPair {
        public VisualEffect target;
        public string eventName;
    }
    
    [SerializeField] private VFXEventPair[] targets;

    public override void Invoke(MonoBehaviour owner){
        base.Invoke(owner);
        foreach (var target in targets) {
            target.target.SendEvent(target.eventName);
        }
    }
}