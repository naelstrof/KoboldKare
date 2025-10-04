using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseAnimatorTrigger : GameEventResponse {
    
    [Serializable]
    public class AnimatorTriggerTarget {
        [SerializeField]
        public Animator animator;
        [SerializeField]
        public string triggerName;
    }

    [SerializeField]
    public AnimatorTriggerTarget[] targets;

    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        foreach (var target in targets) {
            target.animator.SetTrigger(target.triggerName);
        }
    }
}
