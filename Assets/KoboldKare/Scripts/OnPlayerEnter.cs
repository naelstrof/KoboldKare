using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnPlayerEnter : MonoBehaviour {
    public bool entered = false;
    public float delay = 1f;
    [Serializable]
    public class ConditionEventPair {
        [SerializeField]
        private UnityEvent even;
        [SerializeField, SubclassSelector, SerializeReference]
        private List<GameEventResponse> responses = new List<GameEventResponse>();
        public void OnValidate(MonoBehaviour context, string startPath) {
            GameEventSanitizer.SanitizeEditor(startPath+"."+nameof(even), startPath+"."+nameof(responses), context);
        }
        
        public void OnAwake(MonoBehaviour context) {
            GameEventSanitizer.SanitizeRuntime(even, responses, context);
        }

        public bool TryInvoke(MonoBehaviour sender) {
            foreach(var resp in responses) {
                resp?.Invoke(sender);
            }
            return true;
        }
    }

    [SerializeField]
    public List<ConditionEventPair> onEnterEvents;
    [SerializeField]
    public List<ConditionEventPair> onExitEvents;

    private LayerMask playerLayers;

    private void OnValidate() {
        if (onEnterEvents != null) {
            int count = 0;
            foreach (var condition in onEnterEvents) {
                condition.OnValidate(this, "onEnterEvents.Array.data["+(count++)+"]");
            }
        }

        if (onExitEvents != null) {
            int count = 0;
            foreach (var condition in onExitEvents) {
                condition.OnValidate(this, "onExitEvents.Array.data["+(count++)+"]");
            }
        }
    }

    private void Awake() {
        playerLayers = LayerMask.GetMask("Player", "LocalPlayer", "MirrorReflection", "Hitbox");
        foreach (var condition in onEnterEvents) {
            condition.OnAwake(this);
        }
        foreach (var condition in onExitEvents) {
            condition.OnAwake(this);
        }
    }

    IEnumerator OnEnterDelay() {
        yield return new WaitForSeconds(delay);
        foreach (ConditionEventPair pair in onEnterEvents) {
            pair.TryInvoke(this);
        }
    }
    IEnumerator OnExitDelay() {
        yield return new WaitForSeconds(delay);
        foreach (ConditionEventPair pair in onExitEvents) {
            pair.TryInvoke(this);
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
