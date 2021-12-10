using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class KeyTrigger : MonoBehaviour {
    public GameEventGeneric KeyDown;
    public GameEventGeneric KeyUp;
    public string key;
    void Update() {
        if (Input.GetButtonDown(key)) {
            KeyDown?.Raise(null);
        }
        if (Input.GetButtonUp(key)) {
            KeyUp?.Raise(null);
        }
    }
}
