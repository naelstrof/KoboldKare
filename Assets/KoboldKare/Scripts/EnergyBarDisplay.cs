using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarDisplay : MonoBehaviour {
    [SerializeField] private Renderer energyBar;
    [SerializeField] private Renderer energyBarContainer;
    [SerializeField] private AnimationCurve bounceCurve;

    private float desiredValue;
    private float desiredMaxValue;

    private bool animating = false;

    private static readonly int Value = Shader.PropertyToID("_Value");
    private static readonly int MaxValue = Shader.PropertyToID("_MaxValue");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

    private NetworkedKobold kobold;

    void Start() {
        kobold = GetComponentInParent<NetworkedKobold>();
        kobold.energy.OnChange += OnEnergyChanged;
        kobold.maxEnergy.OnChange += OnMaxEnergyChanged;
        energyBar.material.SetColor(ColorID, energyBar.material.GetColor(ColorID).With(a:0f));
        energyBarContainer.material.SetColor(BaseColorID, energyBarContainer.material.GetColor(BaseColorID).With(a:0f));
        OnEnergyChanged(kobold.energy.Value, kobold.energy.Value, false);
        OnMaxEnergyChanged(kobold.maxEnergy.Value, kobold.maxEnergy.Value, false);
    }

    private void OnDestroy() {
        if (kobold) {
            kobold.energy.OnChange -= OnEnergyChanged;
            kobold.maxEnergy.OnChange -= OnMaxEnergyChanged;
        }
    }

    void OnDisable() {
        energyBar.material.SetColor(ColorID, energyBar.material.GetColor(ColorID).With(a:0f));
        energyBarContainer.material.SetColor(BaseColorID, energyBarContainer.material.GetColor(BaseColorID).With(a:0f));
    }

    void OnEnergyChanged(float oldValue, float newValue, bool asServer) {
        if (Math.Abs(desiredValue - newValue) < 0.01f)  {
            return;
        }

        desiredValue = newValue;
        if (!animating) {
            StopAllCoroutines();
            StartCoroutine(EnergyAnimation(energyBar.material.GetFloat(Value), energyBar.material.GetFloat(MaxValue)));
        }
    }
    
    void OnMaxEnergyChanged(float oldValue, float newValue, bool asServer) {
        if (Math.Abs(desiredMaxValue - newValue) < 0.01f)  {
            return;
        }

        desiredMaxValue = newValue;
        if (!animating) {
            StopAllCoroutines();
            StartCoroutine(EnergyAnimation(energyBar.material.GetFloat(Value), energyBar.material.GetFloat(MaxValue)));
        }
    }

    IEnumerator EnergyAnimation(float fromValue, float fromMaxValue) {
        animating = true;
        energyBar.enabled = true;
        energyBarContainer.enabled = true;
        float startTime = Time.time;
        float duration = 2f;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            float sample = bounceCurve.Evaluate(t);
            float lerpValue = Mathf.Lerp(fromValue, desiredValue, sample);
            float lerpMaxValue = Mathf.Lerp(fromMaxValue, desiredMaxValue, sample);
            transform.localScale = new Vector3( lerpMaxValue * 0.5f, 0.25f, 0.25f);
            energyBar.material.SetColor(ColorID, energyBar.material.GetColor(ColorID).With(a:1f));
            energyBarContainer.material.SetColor(BaseColorID, energyBarContainer.material.GetColor(BaseColorID).With(a:1f));
            energyBar.material.SetFloat(Value, lerpValue);
            energyBar.material.SetFloat(MaxValue, lerpMaxValue);
            yield return null;
        }
        transform.localScale = new Vector3( desiredMaxValue * 0.5f, 0.25f, 0.25f);
        energyBar.material.SetFloat(Value, desiredValue);
        energyBar.material.SetFloat(MaxValue, desiredMaxValue);
        animating = false;

        yield return new WaitForSeconds(3f);
        
        float startFadeTime = Time.time;
        float fadeDuration = 1f;
        while (Time.time < startFadeTime + fadeDuration) {
            float t = (Time.time - startFadeTime) / fadeDuration;
            
            energyBar.material.SetColor(ColorID, energyBar.material.GetColor(ColorID).With(a:1f-t));
            energyBarContainer.material.SetColor(BaseColorID, energyBarContainer.material.GetColor(BaseColorID).With(a:1f-t));
            yield return null;
        }
        energyBar.material.SetColor(ColorID, energyBar.material.GetColor(ColorID).With(a:0f));
        energyBarContainer.material.SetColor(BaseColorID, energyBarContainer.material.GetColor(BaseColorID).With(a:0f));
        energyBar.enabled = false;
        energyBarContainer.enabled = false;
    }
}
