using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseAnimatorBoolToggle : GameEventResponse {
    
    [Serializable]
    private class AnimatorBoolToggleTarget {
        [SerializeField]
        public Animator animator;
        [SerializeField]
        public string boolName;
    }

    [SerializeField]
    private AnimatorBoolToggleTarget[] targets;

    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        foreach (var target in targets) {
            target.animator.SetBool(target.boolName, !target.animator.GetBool(target.boolName));
        }
    }
}
