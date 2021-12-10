using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;
using UnityEngine.InputSystem;

public class KeyTriggerPlayerInput : MonoBehaviour {
    public GameEventGeneric KeyDown;
    public GameEventGeneric KeyUp;
    public PlayerInput controls;
    public bool memory = false;
    public void FixedUpdate() {
        bool key = controls.actions["View Stats"].ReadValue<float>() > 0.5f;
        if (key != memory) {
            if (key) {
                KeyDown?.Raise(null);
            } else {
                KeyUp?.Raise(null);
            }
            memory = key;
        }
    }
}
