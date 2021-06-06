using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KoboldKare {
    public class GameEventListener : MonoBehaviour, IGameEventListener {
        public GameEvent Event;
        public UnityEvent Response;

        private void OnEnable() { Event?.RegisterListener(this); }
        private void OnDestroy() { Event?.UnregisterListener(this); }
        public void OnEventRaised(GameEvent e) {
            if (this == null) {
                return;
            }
            try {
                if (gameObject.activeInHierarchy) { Response.Invoke(); }
            } catch (UnityException ex) {
                Debug.LogError("Game event triggered an exception for object: " + gameObject);
                Debug.LogException(ex, gameObject);
            }
        }
    }
}
