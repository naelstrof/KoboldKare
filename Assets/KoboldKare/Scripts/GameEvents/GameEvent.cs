using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KoboldKare {
    [System.Serializable]
    public class GameEvent<T> : ScriptableObject {
        List<GameEventActionGeneric> addLater = new List<GameEventActionGeneric>();
        List<GameEventActionGeneric> removeLater = new List<GameEventActionGeneric>();
        [NonSerialized]
        private bool running = false;
        public delegate void GameEventActionGeneric(T arg);
        private event GameEventActionGeneric raised;

        public void AddListener(GameEventActionGeneric listener) {
            if (running) {
                addLater.Add(listener);
            } else {
                raised += listener;
            }
        }
        public void RemoveListener(GameEventActionGeneric listener) {
            if (running) {
                removeLater.Add(listener);
            } else {
                raised -= listener;
            }
        }

        public void Raise(T arg) {
            if (running) {
                return;
            }

            foreach(var action in addLater) {
                raised += action;
            }
            addLater.Clear();

            running = true;
            raised?.Invoke(arg);
            running = false;

            foreach(var action in removeLater) {
                raised -= action;
            }
            removeLater.Clear();
        }
    }
}
