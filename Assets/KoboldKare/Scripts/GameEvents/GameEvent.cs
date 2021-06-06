using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KoboldKare {

    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Data/GameEvent", order = 1)]
    [System.Serializable]
    public class GameEvent : ScriptableObject {
        [NonSerialized]
        private List<IGameEventListener> listeners = new List<IGameEventListener>();
        [NonSerialized]
        private List<IGameEventListener> savedListeners = new List<IGameEventListener>();
        [NonSerialized]
        private List<IGameEventListener> savedRemovedListeners = new List<IGameEventListener>();
        [NonSerialized]
        private bool running = false;
        [NonSerialized]
        private bool active = true;

        public void RegisterListener(IGameEventListener listener) {
            if (listeners.Contains(listener)) {
                return;
            }
            if (running) {
                savedListeners.Add(listener);
            } else {
                listeners.Add(listener);
            }
        }
        public void UnregisterListener(IGameEventListener listener) {
            if (running) {
                savedRemovedListeners.Add(listener);
            } else {
                listeners.Remove(listener);
            }
        }

        public void Raise() {
            if (running || !active) {
                return;
            }
            listeners.AddRange(savedListeners);
            savedListeners.Clear();

            running = true;
            int size = listeners.Count;
            foreach (IGameEventListener l in listeners) {
                try {
                    l.OnEventRaised(this);
                } catch( Exception e ) {
                    Debug.LogException(e, (Component)l);
                    savedRemovedListeners.Add(l);
                }
            }
            running = false;

            foreach (IGameEventListener l in savedRemovedListeners) {
                listeners.Remove(l);
            }
            savedRemovedListeners.Clear();
        }
    }
}
