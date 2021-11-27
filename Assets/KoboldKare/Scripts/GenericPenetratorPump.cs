using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericPenetratorPump : MonoBehaviour {
    public FluidOutput fluidOutput;
    public GenericReagentContainer balls;
    public PenetrationTech.Penetrator targetPenetrator;
    public ReagentData.ID precumType;
    public float stimulation;
    public float stimulationNeeded = 4f;
    void Start() {
        targetPenetrator.OnCumEmit.AddListener(()=>{
            ReagentContents cumbucket = new ReagentContents();
            cumbucket.Mix(balls.contents.Spill(balls.contents.maxVolume/targetPenetrator.cumPulseCount));
            cumbucket.Mix(precumType, targetPenetrator.dickRoot.transform.lossyScale.x);
            if (!targetPenetrator.IsInside()) {
                targetPenetrator.GetComponentInChildren<FluidOutput>().Fire(cumbucket);
            } else {
                targetPenetrator.holeTarget.GetComponentInParent<Kobold>().bellies[0].container.contents.Mix(cumbucket);
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
