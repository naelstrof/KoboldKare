using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KoboldKare {
    [System.Serializable]
    public class GameEvent<T> : ScriptableObject {
        public delegate void GameEventActionGeneric(T arg);
        private event GameEventActionGeneric raised;
        [NonSerialized]
        private T lastInvokeValue;

        public void AddListener(GameEventActionGeneric listener) {
            raised += listener;
        }
        public void RemoveListener(GameEventActionGeneric listener) {
            raised -= listener;
        }

        public T GetLastInvokeValue() {
            return lastInvokeValue;
        }

        public void Raise(T arg) {
            lastInvokeValue = arg;
            raised?.Invoke(arg);
        }
    }
}
