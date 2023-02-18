using System.Collections;
using UnityEngine;

public class DayNightCycle : MonoBehaviour {
    private static DayNightCycle instance = null;
    private WaitForSeconds waitForTwoSeconds;
    public delegate void MetabolizeAction(float time);
    private event MetabolizeAction metabolizationTriggered;

    public static void AddMetabolizationListener(MetabolizeAction action) {
        if (instance == null) {
            return;
        }

        instance.metabolizationTriggered += action;
    }
    public static void RemoveMetabolizationListener(MetabolizeAction action) {
        if (instance == null) {
            return;
        }
        instance.metabolizationTriggered -= action;
    }

    private void Awake() {
        waitForTwoSeconds = new WaitForSeconds(2f);
        if (instance == null || instance == this) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    private void Start() {
        StartCoroutine(MetabolizeOccassionally());
    }
    private IEnumerator MetabolizeOccassionally() {
        while (isActiveAndEnabled) {
            yield return waitForTwoSeconds;
            metabolizationTriggered?.Invoke(2f);
        }
    }
}
