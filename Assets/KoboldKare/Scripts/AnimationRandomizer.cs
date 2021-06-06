using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRandomizer : StateMachineBehaviour {
    public int min = 0;
    public int max = 1;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex) {
        animator.SetInteger("RandomInt", Random.Range(min, max));
    }
}
