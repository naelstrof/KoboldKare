using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayTrigger : MonoBehaviour {
    [SerializeField, HideInInspector]
    private UnityEvent onTrigger;
    [SerializeField, SubclassSelector, SerializeReference]
    private List<GameEventResponse> onTriggerResponses = new List<GameEventResponse>();
    
    
    public float waitTime = 1f;
    public float waitVariance = 1f;
    private float timer = 0f;
    private void Awake() {
        GameEventSanitizer.SanitizeRuntime(onTrigger, onTriggerResponses, this);
    }

    private void OnValidate() {
        GameEventSanitizer.SanitizeEditor(nameof(onTrigger), nameof(onTriggerResponses), this);
    }

    void Start() {
        waitTime += Random.Range(-waitVariance, waitVariance);
    }
    void FixedUpdate() {
        timer += Time.fixedDeltaTime;
        if ( timer > waitTime ) {
            foreach (GameEventResponse response in onTriggerResponses) {
                response?.Invoke(this);
            }
            Destroy(this);
        }
    }
}
