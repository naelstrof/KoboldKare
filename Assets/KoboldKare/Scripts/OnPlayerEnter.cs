using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnPlayerEnter : MonoBehaviour {
    public bool entered = false;
    public float delay = 1f;
    [Serializable]
    public class Condition : SerializableCallback<bool> { }
    [Serializable]
    public class ConditionEventPair {
        [SerializeField]
        public List<Condition> conditions;
        public UnityEvent even;
    }

    [SerializeField]
    public List<ConditionEventPair> onEnterEvents;
    [SerializeField]
    public List<ConditionEventPair> onExitEvents;

    private LayerMask playerLayers;

    private void Awake() {
        playerLayers = LayerMask.GetMask("Player", "LocalPlayer", "MirrorReflection", "Hitbox");
    }

    IEnumerator OnEnterDelay() {
        yield return new WaitForSeconds(delay);
        foreach (ConditionEventPair pair in onEnterEvents) {
            bool run = true;
            foreach(var cond in pair.conditions) {
                run &= cond.Invoke();
            }
            if (run) {
                pair.even.Invoke();
            }
        }
    }
    IEnumerator OnExitDelay() {
        yield return new WaitForSeconds(delay);
        foreach (ConditionEventPair pair in onExitEvents) {
            bool run = true;
            foreach(var cond in pair.conditions) {
                run &= cond.Invoke();
            }
            if (run) {
                pair.even.Invoke();
            }
        }
    }

    public void OnTriggerEnter(Collider other) {
        if ( (1<<other.gameObject.layer & playerLayers) == 0) {
            return;
        }
        CharacterDescriptor p = other.transform.GetComponentInParent<CharacterDescriptor>();
        if (p != null && p.GetPlayerControlled() == CharacterDescriptor.ControlType.LocalPlayer) {
            StopAllCoroutines();
            StartCoroutine(OnEnterDelay());
        }
    }
    public void OnTriggerExit(Collider other) {
        if ( (1<<other.gameObject.layer & playerLayers) == 0) {
            return;
        }

        CharacterDescriptor p = other.transform.GetComponentInParent<CharacterDescriptor>();
        if (p != null && p.GetPlayerControlled() == CharacterDescriptor.ControlType.LocalPlayer) {
            StopAllCoroutines();
            StartCoroutine(OnExitDelay());
        }
    }
}
