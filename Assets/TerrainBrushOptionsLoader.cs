using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainBrush;

namespace UnityScriptableSettings {
public class TerrainBrushOptionsLoader : MonoBehaviour {
    public ScriptableSetting grassSetting;
    void Start() {
        grassSetting.onValueChange -= OnValueChange;
        grassSetting.onValueChange += OnValueChange;
        OnValueChange(grassSetting);
    }
    void OnDestroy() {
        grassSetting.onValueChange -= OnValueChange;
    }

    public void OnValueChange(ScriptableSetting setting) {
        GetComponent<TerrainBrushOverseer>().foliageDensity = 0.7f*setting.value;
        if (GetComponent<TerrainBrushOverseer>().foliageSaveInScene != true) {
            GetComponent<TerrainBrushOverseer>().RegenerateFoliage();
        }
    }
}

}
