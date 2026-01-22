using System;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class GameEventResponseButtonUse : GameEventResponse {
    private static bool stackOverflowCheck = false;
    private static int stackOverflowCount = 0;
    private const int MAX_STACK = 10;
    
    [SerializeField] private ButtonUsable[] targets;

    public override void Invoke(MonoBehaviour owner) {
        bool isRootInvoker = false;
        if (!stackOverflowCheck) {
            stackOverflowCheck = true;
            stackOverflowCount = 0;
            isRootInvoker = true;
        }
        stackOverflowCount++;
        if (stackOverflowCount > MAX_STACK) {
            stackOverflowCount = 0;
            stackOverflowCheck = false;
            return;
        }
        base.Invoke(owner);
        // FIXME FISHNET, untested, probably should only trigger on host?
        var anyKobold = Object.FindAnyObjectByType<NetworkedKobold>();
        foreach (var target in targets) {
            target.GetComponentInParent<NetworkedEntity>().OnUse(anyKobold);
        }
        if (isRootInvoker) {
            stackOverflowCheck = false;
            stackOverflowCount = 0;
        }
    }
}