using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyFloater : MonoBehaviour {
    [SerializeField]
    private TMPro.TMP_Text text;
    private int nextUpdate;
    private const float fadeDistance = 5f;
    public void SetBounds(Bounds target) {
        transform.position = target.center;
        transform.localScale = Vector3.one * target.size.magnitude;
    }
    public void SetText(string newText) {
        text.text = newText;
    }
    void OnEnable() {
        nextUpdate = 64;
        StartCoroutine(UpdateRoutine());
    }
    IEnumerator UpdateRoutine() {
        while (isActiveAndEnabled) {
            // Every few frames we update
            for (int i = 0; i < nextUpdate; i++) {
                yield return null;
            }

            float distance = Vector3.Distance(transform.position, OrbitCamera.GetPlayerIntendedPosition());
            transform.LookAt(OrbitCamera.GetPlayerIntendedPosition(), Vector3.up);
            text.alpha = Mathf.Clamp01(fadeDistance - distance);
            nextUpdate = distance < fadeDistance + 1f ? 1 : 64;
        }
    }
}
