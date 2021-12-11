using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyFloater : MonoBehaviour {
    [SerializeField]
    private Transform textTransform;
    [SerializeField]
    private TMPro.TMP_Text text;
    private Coroutine routine;
    private int nextUpdate;
    private const float fadeDistance = 10f;
    public void SetBounds(Bounds target) {
        textTransform.position = target.center;
        textTransform.localScale = Vector3.one * target.size.magnitude;
    }
    public void SetText(string newText) {
        text.text = newText;
    }
    void OnEnable() {
        routine = StartCoroutine(UpdateRoutine());
    }
    void OnDisable() {
        if (routine != null) {
            StopCoroutine(routine);
        }
    }
    IEnumerator UpdateRoutine() {
        while(isActiveAndEnabled) {
            // Every few frames we update
            for(int i=0;i<nextUpdate;i++) {
                yield return null;
            }
            // Skip if camera is null
            if (Camera.main == null) {
                nextUpdate = 64;
                continue;
            }
            float distance = textTransform.DistanceTo(Camera.main.transform);
            textTransform.LookAt(Camera.main.transform, Vector3.up);
            text.alpha = Mathf.Clamp01(fadeDistance-distance);
            if (distance < fadeDistance+1f) {
                nextUpdate = 1;
            } else {
                nextUpdate = 64;
            }
        }
    }
}
