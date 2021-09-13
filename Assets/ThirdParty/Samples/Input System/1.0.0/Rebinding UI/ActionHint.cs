using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using KoboldKare;

public class ActionHint : MonoBehaviour {
    public InputActionReference action;
    public TMPro.TextMeshProUGUI text;
    public Image image;
    private Task switchIconTask;
    private bool switching = false;
    private enum State {
        KeyboardMouse,
        Gamepad,
    }
    private State state = State.KeyboardMouse;
    private void SwitchToIndex(State bindingIndex) {
        switching = true;
        state = bindingIndex;
        string displayString = action.action.bindings[(int)bindingIndex].path;
        if (isActiveAndEnabled) {
            if (switchIconTask != null && switchIconTask.Running) {
                return;
            }
            switchIconTask = new Task(WaitThenCheckKey(displayString));
        }
    }
    private void OnActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.BoundControlsChanged)
            return;

        SwitchToIndex(state);
    }
    void OnEnable() {
        InputSystem.onActionChange -= OnActionChange;
        InputSystem.onActionChange += OnActionChange;
        SwitchToIndex(state);
    }
    void OnDisable() {
        InputSystem.onActionChange -= OnActionChange;
    }

    public void Update() {
        if (switching) {
            return;
        }
        if (Gamepad.current != null) {
            if (Gamepad.current.leftStick.IsActuated(0.25f) || Gamepad.current.rightStick.IsActuated(0.25f) || Gamepad.current.buttonSouth.IsPressed()) {
                SwitchToIndex(State.Gamepad);
                return;
            }
        }
        if ((!Keyboard.current.CheckStateIsAtDefaultIgnoringNoise() || Gamepad.current == null) && state == State.Gamepad) {
            SwitchToIndex(State.KeyboardMouse);
            return;
        }
    }
    IEnumerator WaitThenCheckKey(string key) {
        if (key != "") {
            // Wait for the localization system to initialize, loading Locales, preloading etc.
            var otherAsync = LocalizationSettings.SelectedLocaleAsync;
            yield return new WaitUntil(()=>otherAsync.IsDone);
            if (otherAsync.Result != null){
                yield return LocalizationSettings.InitializationOperation;
                var asyncOp = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<Sprite>("InputTexturesTable", key);
                yield return new WaitUntil(()=>asyncOp.IsDone);
                if (asyncOp.IsValid() && asyncOp.Result != null) {
                    text.text = "";
                    image.color = new Color(1,1,1,1f);
                    image.sprite = asyncOp.Result;
                    image.preserveAspect = true;
                } else {
                    text.text = key;
                    image.color = new Color(1,1,1,0f);
                }
            }
        }
        switching = false;
    }
}
