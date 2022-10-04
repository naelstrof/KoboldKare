using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandTextDisplay : MonoBehaviour {
    [SerializeField]
    private TMPro.TMP_Text outputText;
    
    [SerializeField]
    private CanvasGroup group;

    private bool forceVisible;
    private WaitForSeconds waitForSeconds;

    private void Awake() {
        waitForSeconds = new WaitForSeconds(5f);
    }

    void OnEnable() {
        CheatsProcessor.AddOutputChangedListener(OnTextOutputChanged);
    }
    private void OnDisable() {
        CheatsProcessor.RemoveOutputChangedListener(OnTextOutputChanged);
    }
    void OnTextOutputChanged(string message) {
        group.interactable = true;
        group.alpha = 1f;
        outputText.text = message;
        StopAllCoroutines();
        StartCoroutine(WaitThenRemove());
    }

    public void ForceVisible(bool visible) {
        if (visible) {
            StopAllCoroutines();
            group.interactable = true;
            group.alpha = 1f;
        } else {
            if (isActiveAndEnabled) {
                StopAllCoroutines();
                StartCoroutine(WaitThenRemove());
            } else {
                group.interactable = false;
                group.alpha = 0f;
            }
        }

        forceVisible = visible;
    }

    IEnumerator WaitThenRemove() {
        group.interactable = false;
        yield return waitForSeconds;
        yield return new WaitUntil(() => !forceVisible);
        float startTime = Time.time;
        float duration = 1f;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            group.alpha = 1f - t;
            yield return null;
        }
        group.interactable = false;
        group.alpha = 0f;
    }
}
