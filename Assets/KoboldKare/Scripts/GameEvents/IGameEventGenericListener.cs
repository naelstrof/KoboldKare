using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KoboldKare {
    public interface IGameEventGenericListener<T> {
        void OnEventRaised(GameEventGeneric<T> e, T t);
    }
}
