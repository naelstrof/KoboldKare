using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainBrush;

public class TerrainBrushOptionsLoader : MonoBehaviour, IGameEventOptionListener {
    void Start() {
        GraphicsOptions.instance.RegisterListener(this);
        foreach(GraphicsOptions.Option o in GraphicsOptions.instance.options) {
            OnEventRaised(o.type, o.value);
        }
    }
    void OnDestroy() {
        GraphicsOptions.instance.UnregisterListener(this);
    }

    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        if (e!=GraphicsOptions.OptionType.Grass) {
            return;
        }
        GetComponent<TerrainBrushOverseer>().foliageDensity = 0.7f*value;
        if (GetComponent<TerrainBrushOverseer>().foliageSaveInScene != true) {
            GetComponent<TerrainBrushOverseer>().RegenerateFoliage();
        }
    }
}
