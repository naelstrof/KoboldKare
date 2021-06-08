using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GraphicsOptionsEventListener : MonoBehaviour, IGameEventOptionListener {
    public GraphicsOptions.OptionType type;
    public UnityEvent OnEnable;
    public UnityEvent OnDisable;
    [System.Serializable]
    public class UnityEventFloat : UnityEvent<float> { };
    public UnityEventFloat OnChanged;
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
        if (e!=type) {
            return;
        }
        OnChanged.Invoke(value);
        if (Mathf.Approximately(value,0f)) {
            OnDisable.Invoke();
        } else {
            OnEnable.Invoke();
        }
    }
}
