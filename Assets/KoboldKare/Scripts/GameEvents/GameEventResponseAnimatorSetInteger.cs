using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseAnimatorSetInteger : GameEventResponse
{
    [Serializable]
    private struct AnimatorIntTarget
    {
        [SerializeField] public Animator animator;
        [SerializeField] public string intName;
        [SerializeField] public int intValue;
    }

    [SerializeField] private AnimatorIntTarget[] targets;

    public override void Invoke(MonoBehaviour owner)
    {
        base.Invoke(owner);
        foreach (var target in targets) {
            target.animator.SetInteger(target.intName, target.intValue);
        }
    }
}