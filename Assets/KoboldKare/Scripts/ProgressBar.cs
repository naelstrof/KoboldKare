using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBar : MonoBehaviour {
    [SerializeField]
    private RectTransform outline;
    [SerializeField]
    private RectTransform progressBar;

    private void Awake() {
        SetProgress(0f);
    }

    public void SetProgress(float progress) {
        var outlineSizeDelta = outline.sizeDelta;
        progressBar.sizeDelta = new Vector2(outlineSizeDelta.x*progress, outlineSizeDelta.y);
    }
}
