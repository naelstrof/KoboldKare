using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModInfoDisplaySpawner : MonoBehaviour {
    [SerializeField]
    private ModInfoDisplay uiPrefab;
    
    private List<ModInfoDisplay> displays;
    void Start() {
        Refresh();
    }

    public void Refresh() {
        displays ??= new List<ModInfoDisplay>();
        foreach (var modInfoDisplay in displays) {
            Destroy(modInfoDisplay.gameObject);
        }
        displays.Clear();
        foreach (var modInfo in ModManager.GetFullModList()) {
            GameObject obj = Instantiate(uiPrefab.gameObject, transform);
            if (obj.TryGetComponent(out ModInfoDisplay display)) {
                display.SetModInfo(this,modInfo);
                displays.Add(display);
            }
        }
    }
}
