using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Reagent", menuName = "Data/Reagent", order = 1)]
public class ScriptableReagent : ScriptableObject {
    [System.Serializable]
    public class UnityReagentContainerEvent : UnityEvent<GenericReagentContainer> {}
    public LocalizedString localizedName;
    public Color color;
    [ColorUsage(false, true)]
    public Color emission;
    public float value;
    [Tooltip("If the chemical is metabolized, if it should take up a kobold's limited metabolization ability.")]
    public bool consumesMetabolization;
    public float metabolizationHalfLife;
    public bool cleaningAgent;
    public float calories = 0f;
    public GameObject display;
    public Reagent GetReagent( float volume ) {
        return new Reagent() {
            id = ReagentDatabase.GetID(this),
            volume = volume,
        };
    }
}
