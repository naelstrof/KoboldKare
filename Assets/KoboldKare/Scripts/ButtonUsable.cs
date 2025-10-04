using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonUsable : GenericUsable
{
    [SerializeField, HideInInspector] public UnityEvent onUse;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onUseResponses = new List<GameEventResponse>();

    private void Awake() {
        GameEventSanitizer.SanitizeRuntime(onUse, onUseResponses, this);
    }
    
    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(onUse), nameof(onUseResponses), this);
    }

    public override void Use()
    {
        base.Use();
        foreach(var gameEventResponse in onUseResponses) {
            gameEventResponse?.Invoke(this);
        }
    }
}
