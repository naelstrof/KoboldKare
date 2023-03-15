using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModInfoDisplaySpawner : MonoBehaviour {
    [SerializeField]
    private ModInfoDisplay uiPrefab;
    [SerializeField]
    private GameObject noModsFound;
    
    private List<ModInfoDisplay> displays;
    void OnEnable() {
        Refresh();
        ModManager.AddFinishedLoadingListener(Refresh);
    }

    private void OnDisable() {
        ModManager.RemoveFinishedLoadingListener(Refresh);
    }

    private void Refresh() {
        displays ??= new List<ModInfoDisplay>();
        foreach (var modInfoDisplay in displays) {
            Destroy(modInfoDisplay.gameObject);
        }
        displays.Clear();
        noModsFound.SetActive(ModManager.GetFullModList().Count == 0);
        foreach (var modInfo in ModManager.GetFullModList()) {
            GameObject obj = Instantiate(uiPrefab.gameObject, transform);
            if (obj.TryGetComponent(out ModInfoDisplay display)) {
                display.SetModInfo(this,modInfo);
                displays.Add(display);
            }
        }
    }
}
