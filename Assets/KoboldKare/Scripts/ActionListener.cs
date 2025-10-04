using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ActionListener : MonoBehaviour {
    [SerializeField]
    private InputActionReference action;
    
    [SerializeField, HideInInspector] private UnityEvent onPerformed;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onPerformedResponses = new List<GameEventResponse>();

    void Awake() {
        GameEventSanitizer.SanitizeRuntime(onPerformed, onPerformedResponses, this);
    }
    private void OnEnable() {
        action.action.performed += OnPerformed;
    }

    private void OnDisable() {
        action.action.performed -= OnPerformed;
    }

    void OnPerformed(InputAction.CallbackContext ctx) {
        foreach(var gameEventResponse in onPerformedResponses) {
            gameEventResponse?.Invoke(this);
        }
    }

    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(onPerformed), nameof(onPerformedResponses), this);
    }
}
