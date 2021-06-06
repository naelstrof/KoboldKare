using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;
using KoboldKare;

public class LightGraphicsOptionsLoader : MonoBehaviour, IGameEventOptionListener {
    public GraphicsOptions options;
    void Start() {
        options.RegisterListener(this);
        /*foreach(GraphicsOptions.Option o in options.options) {
            if (o.type != GraphicsOptions.OptionType.Shadows) {
                continue;
            }
            GetComponent<HDAdditionalLightData>().shadowResolution.level = Mathf.FloorToInt(o.value);
        }*/
    }
    void OnDestroy() {
        options.UnregisterListener(this);
    }
    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        /*if (e != GraphicsOptions.OptionType.Shadows){
            return;
        }
        GetComponent<HDAdditionalLightData>().shadowResolution.level = Mathf.FloorToInt(value);*/
    }
}
