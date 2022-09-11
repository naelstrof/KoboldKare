using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Reagent", menuName = "Data/Reagent", order = 1)]
public class ScriptableReagent : ScriptableObject {
    [SerializeField]
    private LocalizedString localizedName;
    [SerializeField]
    private Color color;
    [SerializeField, ColorUsage(false, true)]
    private Color emission;
    [SerializeField]
    private float value;
    [SerializeField]
    private float metabolizationHalfLife;
    [SerializeField]
    private bool cleaningAgent;
    [SerializeField]
    private float calories = 0f;
    [SerializeField]
    private GameObject display;
    [SerializeField, SerializeReference, SerializeReferenceButton]
    private ReagentConsumptionEvent consumptionEvent;

    public LocalizedString GetLocalizedName() => localizedName;
    public Color GetColor() => color;
    public Color GetColorEmission() => emission;
    public float GetValue() => value;
    public float GetMetabolizationHalfLife() => metabolizationHalfLife;
    public bool IsCleaningAgent() => cleaningAgent;
    public float GetCalories() => calories;
    public GameObject GetDisplayPrefab() => display;
    public ReagentConsumptionEvent GetConsumptionEvent() => consumptionEvent;
    public Reagent GetReagent( float volume ) {
        return new Reagent() {
            id = ReagentDatabase.GetID(this),
            volume = volume,
        };
    }

    private void OnValidate() {
        consumptionEvent.OnValidate();
    }
}
