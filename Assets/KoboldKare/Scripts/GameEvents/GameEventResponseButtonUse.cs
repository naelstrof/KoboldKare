using System;
using UnityEngine;

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
        foreach (var target in targets) {
            target.Use();
        }
        if (isRootInvoker) {
            stackOverflowCheck = false;
            stackOverflowCount = 0;
        }
    }
}