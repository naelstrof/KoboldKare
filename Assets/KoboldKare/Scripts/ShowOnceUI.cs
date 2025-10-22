using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowOnceUI : MonoBehaviour {
    private static bool hasShown = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init() {
        hasShown = false;
    }

    private void OnEnable() {
        if (hasShown) {
            gameObject.SetActive(false);
        }
    }

    void OnDisable() {
        hasShown = true;
    }
}
