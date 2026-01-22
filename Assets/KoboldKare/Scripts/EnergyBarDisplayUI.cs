using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarDisplayUI : MonoBehaviour {
    [SerializeField] private float energyToPixel = 30f;
    [SerializeField] private RectTransform filledBar;
    [SerializeField] private RectTransform barBackground;
    [SerializeField] private AnimationCurve bounceCurve;
    private NetworkedKobold targetKobold;
    
    private float targetWidth;
    private float targetBackgroundWidth;
    private bool running = false;
    private void OnEnable() {
        targetKobold = GetComponentInParent<NetworkedKobold>();
        targetKobold.energy.OnChange += OnEnergyChanged;
        running = false;
        OnEnergyChanged(targetKobold.energy.Value, targetKobold.energy.Value, false);
        OnMaxEnergyChanged(targetKobold.maxEnergy.Value, targetKobold.maxEnergy.Value, false);
    }

    private void OnDisable() {
        if (targetKobold) {
            targetKobold.energy.OnChange -= OnEnergyChanged;
            targetKobold.maxEnergy.OnChange -= OnEnergyChanged;
        }

        running = false;
    }

    void OnEnergyChanged(float prev, float next, bool asServer) {
        targetWidth = Mathf.Min(next * energyToPixel,3000f);
        if (!running) {
            StartCoroutine(EnergyLerpRoutine(filledBar.sizeDelta.x, barBackground.sizeDelta.x));
        }
    }
    void OnMaxEnergyChanged(float prev, float next, bool asServer) {
        targetBackgroundWidth = Mathf.Min(next * energyToPixel,3000f);
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
            filledBar.sizeDelta = new Vector2(Mathf.Clamp(Mathf.LerpUnclamped(startWidth, targetWidth, bounceSample), 0f, float.MaxValue),
                filledBar.sizeDelta.y);
            barBackground.sizeDelta = new Vector2(Mathf.Clamp(Mathf.LerpUnclamped(startBackgroundWidth, targetBackgroundWidth, bounceSample), 0f, float.MaxValue),
                barBackground.sizeDelta.y);
            yield return null;
        }
        filledBar.sizeDelta = new Vector2(targetWidth, filledBar.sizeDelta.y);
        barBackground.sizeDelta = new Vector2( targetBackgroundWidth, barBackground.sizeDelta.y);
        running = false;
    }
}
