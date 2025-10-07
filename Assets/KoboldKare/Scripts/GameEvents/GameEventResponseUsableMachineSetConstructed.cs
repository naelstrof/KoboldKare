using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseUsableMachineSetConstructed : GameEventResponse
{
    [Serializable]
    public struct ConstructedSetTarget {
        [SerializeField] public UsableMachine targetMachine;
        [SerializeField] public bool beConstructed;
    }

    [SerializeField] private ConstructedSetTarget[] targets;
    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        foreach (var target in targets) {
            target.targetMachine.SetConstructed(target.beConstructed);
        }
    }
}