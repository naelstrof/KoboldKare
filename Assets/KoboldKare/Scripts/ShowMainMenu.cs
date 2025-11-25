using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMainMenu : MonoBehaviour {
    void Start() {
        if (MainMenu.GetCurrentMode() != MainMenu.MainMenuMode.MainMenu) {
            MainMenu.ShowMenuStatic(MainMenu.MainMenuMode.MainMenu);
            Pauser.SetPaused(false);
        }
    }
}
