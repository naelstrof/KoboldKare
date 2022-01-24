using UnityEngine;

public class IrrigationTankHelper : MonoBehaviour{
    public GenericReagentContainer grc;
    public ScriptableReagent reagent;
    public void InjectFluidsBack(){
        grc.AddMix(reagent,10000f,GenericReagentContainer.InjectType.Inject);
    }
}