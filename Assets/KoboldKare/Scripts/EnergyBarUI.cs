using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUI : MonoBehaviour {
    [SerializeField]
    private Kobold targetKobold;
    [SerializeField]
    private Sprite energyBarSprite;
    [SerializeField]
    private Color energyColor;
    [SerializeField]
    private Color deadColor;
    [SerializeField]
    private AnimationCurve flashCurve;

    private NetworkedKobold networkedKobold;
    private float maxEnergy;
    private float energy;

    private List<Image> energyBars;
    [SerializeField]
    private int size = 18;
    
    private void Start() {
        energyBars = new List<Image>();
        networkedKobold = targetKobold.GetComponentInParent<NetworkedKobold>();
        if (networkedKobold) {
            networkedKobold.energy.OnChange += OnEnergyChanged;
            OnEnergyChanged(networkedKobold.energy.Value, networkedKobold.energy.Value, false);
            OnMaxEnergyChanged(networkedKobold.maxEnergy.Value, networkedKobold.maxEnergy.Value, false);
        }
    }

    private void OnEnergyChanged(float prev, float next, bool asServer) {
        energy = next;
        OnChange();
    }

    private void OnChange() {
        StopAllCoroutines();
        // Ensure we have all our bars available.
        for (int i = energyBars.Count; i < maxEnergy; i++) {
            Image img = new GameObject("EnergyBar", typeof(Image)).GetComponent<Image>();
            img.sprite = energyBarSprite;
            img.color = deadColor;
            img.preserveAspect = true;
            img.transform.SetParent(transform, false);
            img.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
            energyBars.Add(img);
        }
        // Delete bars if we somehow lost a max energy.
        for (int i = energyBars.Count - 1; i > maxEnergy; i--) {
            Destroy(energyBars[i].gameObject);
        }
        
        // Then we flash.
        for (int i = 0; i < energyBars.Count; i++) {
            if (isActiveAndEnabled) {
                StartCoroutine(FlashChange(energyBars[i], i < energy ? energyColor : deadColor));
            } else {
                energyBars[i].color =  i < energy ? energyColor : deadColor;
            }
        }
    }
    
    private void OnMaxEnergyChanged(float prev, float next, bool asServer) {
        maxEnergy = next;
        OnChange();
    }

    private IEnumerator FlashChange(Image target, Color newColor) {
        if (target.color == newColor) {
            yield break;
        }
        float startTime = Time.unscaledTime;
        float duration = 1f;
        Color oldColor = target.color;
        while (Time.unscaledTime < startTime + duration) {
            float t = (Time.unscaledTime - startTime) / duration;
            float sample = flashCurve.Evaluate(t);
            target.color = Color.LerpUnclamped(oldColor, newColor, sample);
            yield return null;
        }
        target.color = newColor;
    }
}
