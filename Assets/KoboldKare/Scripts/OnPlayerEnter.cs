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
        if ( other.gameObject.layer != LayerMask.NameToLayer("Player")) {
            return;
        }
        if (other.transform.root.gameObject.layer != LayerMask.NameToLayer("Player")) {
            return;
        }
        PlayerPossession p = other.transform.root.GetComponentInChildren<PlayerPossession>();
        if (p != null && p.gameObject.activeInHierarchy) {
            StopAllCoroutines();
            StartCoroutine(OnEnterDelay());
        }
    }
    public void OnTriggerExit(Collider other) {
        if ( other.gameObject.layer != LayerMask.NameToLayer("Player")) {
            return;
        }
        if (other.transform.root.gameObject.layer != LayerMask.NameToLayer("Player")) {
            return;
        }
        PlayerPossession p = other.transform.root.GetComponentInChildren<PlayerPossession>();
        if (p != null &&p.gameObject.activeInHierarchy) {
            StopAllCoroutines();
            StartCoroutine(OnExitDelay());
        }
    }
}
