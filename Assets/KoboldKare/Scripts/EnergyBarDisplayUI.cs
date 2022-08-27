using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarDisplayUI : MonoBehaviour {
    [SerializeField] private float energyToPixel = 30f;
    [SerializeField] private RectTransform filledBar;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private AnimationCurve bounceCurve;
    [SerializeField] private Kobold targetKobold;
    
    private float targetWidth;
    private float targetBackgroundWidth;
    private bool running = false;
    private void OnEnable() {
        targetKobold.energyChanged += OnEnergyChanged;
        running = false;
        OnEnergyChanged(targetKobold.GetEnergy(), targetKobold.GetMaxEnergy());
    }

    private void OnDisable() {
        targetKobold.energyChanged -= OnEnergyChanged;
        running = false;
    }

    void OnEnergyChanged(int energy, int maxEnergy) {
        targetWidth = energy * energyToPixel;
        targetBackgroundWidth = maxEnergy * energyToPixel;
        if (!running) {
            StartCoroutine(EnergyLerpRoutine(filledBar.sizeDelta.x, barBackground.sizeDelta.x));
        }
    }

    IEnumerator EnergyLerpRoutine(float startWidth, float startBackgroundWidth) {
        running = true;
        float startTime = Time.time;
        float duration = 1f;
        while (Time.time < startTime+duration) {
            float t = (Time.time - startTime) / duration;
            float bounceSample = bounceCurve.Evaluate(t);
            filledBar.sizeDelta = new Vector2(Mathf.LerpUnclamped(startWidth, targetWidth, bounceSample),
                filledBar.sizeDelta.y);
            barBackground.sizeDelta = new Vector2(Mathf.LerpUnclamped(startBackgroundWidth, targetBackgroundWidth, bounceSample),
                barBackground.sizeDelta.y);
            yield return null;
        }
        filledBar.sizeDelta = new Vector2(targetWidth, filledBar.sizeDelta.y);
        barBackground.sizeDelta = new Vector2( targetBackgroundWidth, barBackground.sizeDelta.y);
        running = false;
    }
}
