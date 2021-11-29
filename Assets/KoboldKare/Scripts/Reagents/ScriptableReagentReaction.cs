using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Reagent", menuName = "Data/Reagent Reaction", order = 1)]
public class ScriptableReagentReaction : ScriptableObject {
    [System.Serializable]
    public class ReagentReactionEvent : UnityEvent<GenericReagentContainer> {}
    public ReagentReactionEvent OnReaction;
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
        float minReactantVolume = container.GetVolumeOf(minReactant.reactant);
        foreach(var reactant in reactants) {
            float reactantVolume = container.GetVolumeOf(reactant.reactant);
            if (reactantVolume < minReactantVolume) {
                minReactant = reactant;
                minReactantVolume = reactantVolume;
            }
        }

        float reactRatio = minReactantVolume / minReactant.coefficient;
        if (reactRatio == 0f) {
            return;
        }
        foreach(var reactant in reactants) {
            float reactantVolume = container.GetVolumeOf(reactant.reactant);
            container.OverrideReagent(reactant.reactant, reactantVolume-reactRatio*reactant.coefficient);
        }
        foreach(var product in products) {
            container.AddMix(product.reactant, reactRatio*product.coefficient, GenericReagentContainer.InjectType.Metabolize);
        }
        OnReaction.Invoke(container);
    }
}
