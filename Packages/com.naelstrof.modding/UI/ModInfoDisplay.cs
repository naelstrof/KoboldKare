using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModInfoDisplay : MonoBehaviour {
    private ModInfo info;
    [SerializeField]
    private Toggle toggle;
    [SerializeField]
    private Button moveUp;
    [SerializeField]
    private Button moveDown;
    [SerializeField]
    private TMP_Text modName;
    [SerializeField]
    private TMP_Text modPath;
    [SerializeField]
    private ModInfoDisplaySpawner modInfoDisplaySpawner;

    public void SetModInfo(ModInfoDisplaySpawner spawner, ModInfo newInfo) {
        info = newInfo;
        modName.text = newInfo.modName;
        modPath.text = newInfo.cataloguePath;
        modInfoDisplaySpawner = spawner;
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(OnToggle);
        moveUp.onClick.RemoveAllListeners();
        moveUp.onClick.AddListener(OnMoveUp);
        moveDown.onClick.RemoveAllListeners();
        moveDown.onClick.AddListener(OnMoveDown);
    }

    private void OnToggle(bool newState) {
        info.enabled = newState;
        ModManager.Reload();
    }

    private void OnMoveUp() {
        ModManager.IncrementPriority(info);
        modInfoDisplaySpawner.Refresh();
        ModManager.Reload();
    }
    
    private void OnMoveDown() {
        ModManager.DecrementPriority(info);
        modInfoDisplaySpawner.Refresh();
        ModManager.Reload();
    }
    
}
