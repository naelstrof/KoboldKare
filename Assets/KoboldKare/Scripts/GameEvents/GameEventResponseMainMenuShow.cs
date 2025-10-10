using System;
using UnityEngine;

[Serializable]
public class GameEventResponseMainMenuShow : GameEventResponse {
    [SerializeField]
    public MainMenu.MainMenuMode menu;

    public override void Invoke(MonoBehaviour owner) {
        base.Invoke(owner);
        MainMenu.ShowMenuStatic(menu);
    }
}
