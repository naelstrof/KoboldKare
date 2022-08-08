using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KoboldKare {
    [System.Serializable]
    public class GameEvent<T> : ScriptableObject {
        public delegate void GameEventActionGeneric(T arg);
        private event GameEventActionGeneric raised;

        public void AddListener(GameEventActionGeneric listener) {
            raised += listener;
        }
        public void RemoveListener(GameEventActionGeneric listener) {
            raised -= listener;
        }

        public void Raise(T arg) {
            raised?.Invoke(arg);
        }
    }
}
