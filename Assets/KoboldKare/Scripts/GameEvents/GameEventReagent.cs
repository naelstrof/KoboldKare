using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;


namespace KoboldKare {
    [CreateAssetMenu(fileName = "NewGameEventReagent", menuName = "Data/GameEvent: ScriptableReagent", order = 2)]
    public class GameEventReagent : GameEvent<ScriptableReagent> {}
}