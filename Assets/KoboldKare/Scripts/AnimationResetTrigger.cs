using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationResetTrigger : StateMachineBehaviour {
    public string trigger;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.ResetTrigger(trigger);
    }
}
