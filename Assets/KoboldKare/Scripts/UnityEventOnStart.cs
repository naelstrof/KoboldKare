using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventOnStart : MonoBehaviour {
    [SerializeField, HideInInspector]
    private UnityEvent OnStart;
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onStartResponses = new List<GameEventResponse>();
    
    void OnAwake() {
        GameEventSanitizer.SanitizeRuntime(OnStart, onStartResponses, this);
    }
    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(OnStart), nameof(onStartResponses), this);
    }

    void Start() {
        foreach (GameEventResponse response in onStartResponses) {
            response?.Invoke(this);
        }
    }
}
