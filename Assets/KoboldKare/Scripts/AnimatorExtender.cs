using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimatorExtender : MonoBehaviour {
    private Animator targetAnimator;
    private AudioSource source;
    void Start() {
        source = GetComponent<AudioSource>();
        targetAnimator = GetComponent<Animator>();
    }
    public void ToggleFloat(string name) {
        targetAnimator.SetFloat(name, targetAnimator.GetFloat(name) <= 0f ? 1f : 0f);
    }
    public void SetBool(string boolName) {
        targetAnimator.SetBool(boolName, true);
    }
    public void ResetBool(string boolName) {
        targetAnimator.SetBool(boolName, false);
    }
    public void PlayAudioPack(AudioPack pack) {
        pack.Play(source);
    }
}
