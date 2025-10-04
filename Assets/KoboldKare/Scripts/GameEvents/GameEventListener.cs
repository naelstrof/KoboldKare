using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

namespace KoboldKare {
    public class GameEventListener <T> : MonoBehaviour {
        public GameEvent<T> Event;
        [SerializeField, HideInInspector]
        private UnityEvent Response;
        
        [SerializeField, SubclassSelector, SerializeReference]
        private List<GameEventResponse> responses = new List<GameEventResponse>();
        
        void Awake() {
            GameEventSanitizer.SanitizeRuntime(Response, responses, this);
        }
        private void OnValidate() {
            GameEventSanitizer.SanitizeEditor(nameof(Response), nameof(responses), this);
        }
        
        private void OnEnable() { Event.AddListener(OnEventRaised); }
        private void OnDisable() { Event.RemoveListener(OnEventRaised); }
        public void OnEventRaised(T arg) {
            if (isActiveAndEnabled) {
                foreach (GameEventResponse response in responses) {
                    response?.Invoke(this);
                }
            }
        }
    }
}
