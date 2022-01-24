using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdultCheckShower : MonoBehaviour {
    public ScriptableFloat shouldShow;
    void Start() {
        if (shouldShow.value == 0) {
            gameObject.SetActive(false);
        }
        shouldShow.set(0);
    }
}
