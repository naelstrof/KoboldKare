using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class PurchaseMachineObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    [SerializeField]
    private UsableMachine targetMachine;
    public override void Register() {
        ConstructionContract.purchasedEvent += OnContractSold;
    }
    public override void Unregister() {
        ConstructionContract.purchasedEvent -= OnContractSold;
    }
    private void OnContractSold(ConstructionContract contract) {
        if (contract is MachineConstructionContract machineContract) {
            if (machineContract.GetMachines().Contains(targetMachine)) {
                TriggerComplete();
            }
        }
    }
    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
