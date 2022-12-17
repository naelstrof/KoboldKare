using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class PurchaseMachineObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;
    [SerializeField]
    private UsableMachine targetMachine;
    public override void Register() {
        base.Register();
        ConstructionContract.purchasedEvent += OnContractSold;
    }
    public override void Unregister() {
        base.Unregister();
        ConstructionContract.purchasedEvent -= OnContractSold;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    private void OnContractSold(ConstructionContract contract) {
        if (contract is MachineConstructionContract machineContract) {
            if (machineContract.GetMachines().Contains(targetMachine)) {
                ObjectiveManager.NetworkAdvance(targetMachine.transform.position, $"{contract.photonView.ViewID.ToString()}");
            }
        }
    }
    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
