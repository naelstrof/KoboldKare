using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KoboldKare {
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Data/GameEvent", order = 1)]
    public class GameEventGeneric : GameEvent<object> { }
}
