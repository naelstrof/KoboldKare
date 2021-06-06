using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericReagentFactory : MonoBehaviour {
    public GenericReagentContainer container;
    [Tooltip("The reagents whos volume drives the time of the animation curve.")]
    public List<ReagentData.ID> reagentMasks;
    public ReagentData.ID generatedReagent;
    public AnimationCurve generatedCurve;
    public void TriggerGeneration(float percent) {
        float volume = 0f;
        foreach( var p in container.contents) {
            if (reagentMasks.Contains(p.Key)) {
                volume += p.Value.volume;
            }
        }
        float desiredVolume = generatedCurve.Evaluate(volume);
        float currentVolume = 0f;
        if (container.contents.ContainsKey(generatedReagent)) {
            currentVolume = container.contents[generatedReagent].volume;
        }
        container.contents.Mix(generatedReagent,  (desiredVolume-currentVolume)*percent, 1f, 310f);
    }
}
