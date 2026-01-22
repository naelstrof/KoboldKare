using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ButtonUsable : MonoBehaviour {
    [SerializeField, HideInInspector] public UnityEvent onUse;
    
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onUseResponses = new List<GameEventResponse>();

    [SerializeField] private Sprite buttonSprite;

    private NetworkedEntity networkedEntity;

    private void Awake() {
        GameEventSanitizer.SanitizeRuntime(onUse, onUseResponses, this);
    }

    private void Start() {
        networkedEntity = GetComponentInParent<NetworkedEntity>();
        if (networkedEntity) {
            networkedEntity.SetSprite(buttonSprite);
            networkedEntity.useRequested += OnUseRequested;
            networkedEntity.used += OnUse;
        }
    }

    private bool OnUseRequested(NetworkedKobold by) {
        return true;
    }

    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(onUse), nameof(onUseResponses), this);
    }

    private void OnUse(NetworkedKobold k) {
        foreach(var gameEventResponse in onUseResponses) {
            gameEventResponse?.Invoke(this);
        }
    }
}
