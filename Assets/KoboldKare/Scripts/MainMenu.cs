using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour {
    private static MainMenu instance;

    [SerializeField] private InputActionReference backButton;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        instance = null;
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        backButton.action.performed += OnBackButton;
    }

    private void OnDestroy() {
        backButton.action.performed -= OnBackButton;
    }

    private void OnBackButton(InputAction.CallbackContext obj) {
        if (LevelLoader.loadingLevel) return;
        if (!LevelLoader.InLevel()) return;
        if (!Pauser.GetPaused()) {
            Pauser.SetPaused(true);
            ShowMenuStatic(MainMenuMode.MainMenu);
        } else {
            Pauser.SetPaused(false);
            ShowMenuStatic(MainMenuMode.None);
        }
    }
    
    [SerializeField]
    private GameObject MultiplayerTab;
    [SerializeField]
    private GameObject OptionsTab;
    [SerializeField]
    private GameObject MainViewTab;
    [SerializeField]
    private GameObject CreditsTab;
    [SerializeField]
    private GameObject MapSelect;
    [SerializeField]
    private GameObject ModdingTab;
    [SerializeField]
    private GameObject SaveTab;
    [SerializeField]
    private GameObject EquipmentTab;
    [SerializeField]
    private GameObject ChatTab;
    [SerializeField]
    private GameObject loadingMenu;

    [System.Serializable]
    public enum MainMenuMode {
        None,
        MainMenu,
        Options,
        MapSelect,
        Multiplayer,
        Credits,
        SaveLoad,
        Loading,
        Equipment,
        Chat,
        Modding,
    }
    
    private MainMenuMode currentMode = MainMenuMode.None;
    public static MainMenuMode GetCurrentMode() => instance.currentMode;

    public void ShowMenu(MainMenuMode mode) {
        if (currentMode == mode) {
            return;
        }
        MultiplayerTab.SetActive(false);
        OptionsTab.SetActive(false);
        MainViewTab.SetActive(false);
        CreditsTab.SetActive(false);
        MapSelect.SetActive(false);
        ModdingTab.SetActive(false);
        SaveTab.SetActive(false);
        ChatTab.SetActive(false);
        loadingMenu.SetActive(false);
        EquipmentTab.SetActive(false);
        PopupHandler.instance.ClearAllPopups();
        switch (mode) {
            case MainMenuMode.MainMenu: MainViewTab.SetActive(true); break;
            case MainMenuMode.Multiplayer: MultiplayerTab.SetActive(true); break;
            case MainMenuMode.Credits: CreditsTab.SetActive(true); break;
            case MainMenuMode.MapSelect: MapSelect.SetActive(true); break;
            case MainMenuMode.Options: OptionsTab.SetActive(true); break;
            case MainMenuMode.SaveLoad: SaveTab.SetActive(true); break;
            case MainMenuMode.Loading: loadingMenu.SetActive(true); break;
            case MainMenuMode.Equipment: EquipmentTab.SetActive(true); break;
            case MainMenuMode.Chat: ChatTab.SetActive(true); break;
            case MainMenuMode.Modding: ModdingTab.SetActive(true); break;
        }

        if (mode != MainMenuMode.None) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        currentMode = mode;
    }
    
    public static void ShowMenuStatic(MainMenuMode mode) {
        instance.ShowMenu(mode);
    }
}
