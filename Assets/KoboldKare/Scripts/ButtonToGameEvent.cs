using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using System;

[Serializable]
public class UIButtonCondition : SerializableCallback<bool> { }
public class ButtonToGameEvent : MonoBehaviour, IGameEventListener {
    public string button;
    public GameEvent ToggleOn;
    public GameEvent ToggleOff;
    private bool toggle = false;
    public List<UIButtonCondition> conditions = new List<UIButtonCondition>();

    public void OnEventRaised(GameEvent e) {
        if (e == ToggleOn) {
            toggle = true;
            return;
        }
        toggle = false;
    }

    private void Awake() {
        ToggleOn.RegisterListener(this);
        ToggleOff.RegisterListener(this);
    }

    private void OnDestroy() {
        ToggleOn.UnregisterListener(this);
        ToggleOff.UnregisterListener(this);
    }

    void Update() {
        if (Input.GetButtonDown(button)) {
            toggle = !toggle;
            foreach(UIButtonCondition condition in conditions) {
                if (!condition.Invoke()) {
                    return;
                }
            }
            if (toggle) {
                ToggleOn.Raise();
            } else {
                ToggleOff.Raise();
            }
        }
    }
}
