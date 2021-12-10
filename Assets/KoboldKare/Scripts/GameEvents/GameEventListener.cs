using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

namespace KoboldKare {
    public class GameEventListener <T> : MonoBehaviour {
        public GameEvent<T> Event;
        public UnityEvent Response;
        private void OnEnable() { Event?.AddListener(OnEventRaised); }
        private void OnDestroy() { Event?.RemoveListener(OnEventRaised); }
        public void OnEventRaised(T arg) {
            if (gameObject.activeInHierarchy) { Response.Invoke(); }
        }
    }
}
