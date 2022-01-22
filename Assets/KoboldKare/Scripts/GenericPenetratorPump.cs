using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericPenetratorPump : MonoBehaviour {
    public GenericReagentContainer balls;
    public PenetrationTech.Penetrator targetPenetrator;
    public ScriptableReagent precum;
    public float stimulation;
    public float stimulationNeeded = 4f;
    void Start() {
        targetPenetrator.OnCumEmit.AddListener(()=>{
            if (!targetPenetrator.IsInside()) {
                targetPenetrator.GetComponentInChildren<IFluidOutput>().Fire(balls);
            } else {
                targetPenetrator.holeTarget.GetComponentInParent<Kobold>().bellies[0].GetContainer().TransferMix(balls, balls.maxVolume/targetPenetrator.cumPulseCount, GenericReagentContainer.InjectType.Inject);
            }
        });
        targetPenetrator.OnMove.AddListener((float amount)=>{
            stimulation += Mathf.Abs(amount);
            if (stimulation > stimulationNeeded) {
                stimulation = -stimulationNeeded;
                targetPenetrator.Cum();
            }
        });
    }
}
