using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KoboldKare;

public class VolumeChecker : MonoBehaviour
{
    public GameEventGeneric trigger;
    bool hasPlayer = false;
    void OnTriggerEnter(Collider c) {
        if ( c.gameObject.CompareTag("Player") ) {
            hasPlayer = true;
        }
    }
    void OnTriggerExit(Collider c) {
        if ( c.gameObject.CompareTag("Player") ) {
            hasPlayer = false;
        }
    }

    public void TriggerIfNotInside() {
        if (!hasPlayer) {
            trigger.Raise(null);
        }
    }
}
