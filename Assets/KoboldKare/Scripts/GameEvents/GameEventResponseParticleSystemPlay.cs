using System;
using UnityEngine;
using UnityEngine.VFX;

[System.Serializable]
public class GameEventResponseParticleSystemPlay : GameEventResponse
{
    [Serializable]
    private class ParticleSystemTarget
    {
        [SerializeField] public VisualEffect VFX;
        [SerializeField] public String possibleEventName;
    }

    [SerializeField] private ParticleSystemTarget[] targets;

    public override void Invoke(MonoBehaviour owner){
        base.Invoke(owner);
        foreach (var target in targets) {
            target.VFX.Play();
            if (target.possibleEventName != null) {
            target.VFX.SendEvent(target.possibleEventName);
            }
        }
    }
}