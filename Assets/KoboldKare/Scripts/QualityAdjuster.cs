using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KoboldKare;

public class QualityAdjuster : MonoBehaviour, IGameEventOptionListener {
    public GraphicsOptions options;
    public GraphicsOptions.OptionType type;
    public UnityEvent onEnable;
    public UnityEvent onDisable;
    void Start() {
        options.RegisterListener(this);
        foreach(GraphicsOptions.Option o in options.options) {
            if (o.type != type) {
                continue;
            }
            OnEventRaised(o.type, o.value);
        }
    }
    void OnDestroy() {
        options.UnregisterListener(this);
    }
    public void OnEventRaised(GraphicsOptions.OptionType e, float value) {
        if (e != type) {
            return;
        }
        if (value == 1f) {
            onEnable.Invoke();
            return;
        }
        onDisable.Invoke();
    }
}
