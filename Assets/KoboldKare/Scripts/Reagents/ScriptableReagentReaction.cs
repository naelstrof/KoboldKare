using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Reagent", menuName = "Data/Reagent Reaction", order = 1)]
public class ScriptableReagentReaction : ScriptableObject {
    [SerializeReference, SerializeReferenceButton]
    private ReagentReaction[] reactions;
    
    [System.Serializable]
    public class Reactant {
        public ScriptableReagent reactant;
        [Range(0.01f,10f)]
        public float coefficient;
    }
    public Reactant[] reactants;
    public Reactant[] products;
    public void DoReaction(GenericReagentContainer container) {
        Reactant minReactant = reactants[0];
        float minReactantVolumeRatio = container.GetVolumeOf(minReactant.reactant)/Mathf.Max(minReactant.coefficient,0.001f);
        foreach(var reactant in reactants) {
            if (reactant.coefficient == 0) {
                continue;
            }
            float reactantVolumeRatio = container.GetVolumeOf(reactant.reactant)/reactant.coefficient;
            if (reactantVolumeRatio < minReactantVolumeRatio) {
                minReactant = reactant;
                minReactantVolumeRatio = reactantVolumeRatio;
            }
        }

        if (Mathf.Approximately(minReactantVolumeRatio, 0f)) {
            return;
        }
        foreach(var reactant in reactants) {
            float reactantVolume = container.GetVolumeOf(reactant.reactant);
            container.OverrideReagent(reactant.reactant, reactantVolume-minReactantVolumeRatio*reactant.coefficient);
        }
        foreach(var product in products) {
            container.GetContents().AddMix(ReagentDatabase.GetID(product.reactant), minReactantVolumeRatio * product.coefficient, container);
        }

        foreach (var reaction in reactions) {
            reaction.React(container);
        }
    }
}
