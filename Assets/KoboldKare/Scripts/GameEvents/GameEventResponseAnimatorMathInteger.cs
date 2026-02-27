using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class GameEventResponseAnimatorMathInteger : GameEventResponse
{
    [Serializable]
    private struct AnimatorIntTarget
    {
        [SerializeField] public Animator animator;
        [SerializeField] public string intName;
        [SerializeField] public int Value;
    }

    [SerializeField] private AnimatorIntTarget[] targets;
    public override void Invoke(MonoBehaviour owner)
    {
        base.Invoke(owner);
        foreach (var target in targets) {
            var currentValue = target.animator.GetInteger(target.intName);
                target.animator.SetInteger(target.intName, currentValue + target.Value);
        }
    }
}