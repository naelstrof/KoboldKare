using System;
using UnityEngine;

[System.Serializable]
public class GameEventResponseButtonUse : GameEventResponse
{
    [Serializable]
    private class ButtonUseTarget
    {
        [SerializeField] public ButtonUsable Button;
    }

    [SerializeField] private ButtonUseTarget[] targets;

    public override void Invoke(MonoBehaviour owner){
        base.Invoke(owner);
        foreach (var target in targets) {
            target.Button.Use();
        }
    }
}