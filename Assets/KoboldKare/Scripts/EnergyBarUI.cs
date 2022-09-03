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

    private List<Image> energyBars;
    [SerializeField]
    private int size = 18;
    
    private void Start() {
        energyBars = new List<Image>();
        targetKobold.energyChanged += OnEnergyChanged;
        OnEnergyChanged(targetKobold.GetEnergy(), targetKobold.GetMaxEnergy());
    }

    private void OnEnergyChanged(float energy, float maxEnergy) {
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
