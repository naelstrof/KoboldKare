using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseAnimatorPlay : GameEventResponse {
    
    [Serializable]
    public class AnimatorPlayTarget {
        [SerializeField]
        public Animator animator;
        [SerializeField]
        public string animationName;
    }

    [SerializeField]
    public AnimatorPlayTarget[] targets;

    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        foreach (var target in targets) {
            target.animator.Play(target.animationName);
        }
    }
}
