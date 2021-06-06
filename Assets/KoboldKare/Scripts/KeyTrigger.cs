using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class KeyTrigger : MonoBehaviour {
    public GameEvent KeyDown;
    public GameEvent KeyUp;
    public string key;
    void Update() {
        if (Input.GetButtonDown(key)) {
            KeyDown?.Raise();
        }
        if (Input.GetButtonUp(key)) {
            KeyUp?.Raise();
        }
    }
}
