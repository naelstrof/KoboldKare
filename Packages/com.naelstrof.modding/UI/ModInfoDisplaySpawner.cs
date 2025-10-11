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
    
    private List<ModInfoDisplay> displays = new();
    void OnEnable() {
        StartCoroutine(WaitThenRefresh());
        ModManager.AddModListChangeListener(Refresh);
        ModManager.AddFinishedLoadingListener(Refresh);
    }

    private void OnDisable() {
        ModManager.RemoveModListChangeListener(Refresh);
        ModManager.RemoveFinishedLoadingListener(Refresh);
    }
    
    private IEnumerator WaitThenRefresh() {
        yield return new WaitUntil(ModManager.GetReady);
        Refresh();
    }

    private void Refresh() {
        displays ??= new List<ModInfoDisplay>();
        foreach (var modInfoDisplay in displays) {
            Destroy(modInfoDisplay.gameObject);
        }
        displays.Clear();
        var list = ModManager.GetFullModList();
        noModsFound.SetActive(list.Count == 0);
        foreach (var modInfo in list) {
            GameObject obj = Instantiate(uiPrefab.gameObject, transform);
            if (obj.TryGetComponent(out ModInfoDisplay display)) {
                display.SetModInfo(this,modInfo);
                displays.Add(display);
            }
        }
    }

    private bool active = true;
    private void Update() {
        if (!ModManager.GetReady() && active) {
            active = false;
            foreach (var display in displays) {
                foreach(var interactable in display.GetComponentsInChildren<Selectable>()) {
                    interactable.interactable = false;
                }
            }
        } else if (ModManager.GetReady() && !active) {
            active = true;
            foreach (var display in displays) {
                foreach(var interactable in display.GetComponentsInChildren<Selectable>()) {
                    interactable.interactable = true;
                }
            }
        }
    }
}
