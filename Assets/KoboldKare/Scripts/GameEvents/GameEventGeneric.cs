using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KoboldKare {
    public class GameEventGeneric<T> : ScriptableObject {
        [NonSerialized]
        private List<IGameEventGenericListener<T>> listeners = new List<IGameEventGenericListener<T>>();
        [NonSerialized]
        private List<IGameEventGenericListener<T>> savedListeners = new List<IGameEventGenericListener<T>>();
        [NonSerialized]
        private List<IGameEventGenericListener<T>> savedRemovedListeners = new List<IGameEventGenericListener<T>>();
        [NonSerialized]
        private bool running = false;
        [NonSerialized]
        private bool active = true;

        public void RegisterListener(IGameEventGenericListener<T> listener) {
            if (running) {
                savedListeners.Add(listener);
            } else {
                listeners.Add(listener);
            }
        }
        public void UnregisterListener(IGameEventGenericListener<T> listener) {
            if (running) {
                savedRemovedListeners.Add(listener);
            } else {
                listeners.Remove(listener);
            }
        }

        public void Raise(T t) {
            if (running || !active) {
                return;
            }
            listeners.AddRange(savedListeners);
            savedListeners.Clear();

            running = true;
            int size = listeners.Count;
            foreach (IGameEventGenericListener<T> l in listeners) {
                try {
                    l.OnEventRaised(this, t);
                } catch (Exception e) {
                    Debug.LogException(e);
                    savedRemovedListeners.Add(l);
                }
            }
            running = false;

            foreach (IGameEventGenericListener<T> l in savedRemovedListeners) {
                listeners.Remove(l);
            }
            savedRemovedListeners.Clear();
        }
    }
}
