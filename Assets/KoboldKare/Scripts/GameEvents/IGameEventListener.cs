using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KoboldKare {
    public interface IGameEventListener {
        void OnEventRaised(GameEvent e);
    }
}
