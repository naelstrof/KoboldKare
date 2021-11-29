using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericReagentFactory : MonoBehaviour {
    public GenericReagentContainer container;
    [Tooltip("The reagents whos volume drives the time of the animation curve.")]
    public ScriptableReagent[] reagentMasks;
    public ScriptableReagent generatedReagent;
    public AnimationCurve generatedCurve;
    public void TriggerGeneration(float percent) {
        float volume = 0f;
        foreach( var mask in reagentMasks) {
            volume += container.GetVolumeOf(mask);
        }
        float desiredVolume = generatedCurve.Evaluate(volume);
        float currentVolume = 0f;
        container.AddMix(generatedReagent, (desiredVolume-currentVolume)*percent, GenericReagentContainer.InjectType.Inject);
    }
}
