using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechBubblePopup : MonoBehaviour {
    [SerializeField]
    private AnimationCurve bounceCurve;
    [SerializeField]
    private RectTransform targetTransform;
    void OnEnable() {
        transform.localScale = Vector3.one * 0.001f;
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine() {
        float startTime = Time.time;
        float duration = 0.4f;
        var sizeDelta = targetTransform.sizeDelta;
        Vector3 desiredScale = new Vector3(sizeDelta.x, sizeDelta.y, 1f);
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            float sample = bounceCurve.Evaluate(t);
            transform.localScale = Vector3.LerpUnclamped(Vector3.one * 0.001f, desiredScale, sample);
            yield return null;
        }
        transform.localScale = desiredScale;
    }
}
