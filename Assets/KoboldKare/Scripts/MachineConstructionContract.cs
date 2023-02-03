using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineConstructionContract : ConstructionContract {
    [SerializeField]
    private UsableMachine[] machines;

    public UsableMachine[] GetMachines() => machines;

    protected override void SetState(bool purchased) {
        base.SetState(purchased);
        foreach (UsableMachine machine in machines) {
            machine.SetConstructed(purchased);
        }
    }
    
    public void ForceState(bool purchased) {
        SetState(purchased);
    }
}
