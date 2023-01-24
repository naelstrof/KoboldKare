using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandEquip : Command {
    public override string GetArg0() => "/equip";
    public override void Execute(StringBuilder output, Kobold kobold, string[] args) {
        base.Execute(output, kobold, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length < 2) {
            throw new CheatsProcessor.CommandException("/equip requires at least one argument. Use `/list equipment` to find what you can equip.");
        }

        Equipment tryEquipment;
        try {
            tryEquipment = EquipmentDatabase.GetEquipment(args[1]);
        } catch (UnityException exception) {
            throw new CheatsProcessor.CommandException(exception.Message);
        }

        if (tryEquipment != null) {
            output.Append($"Equipped {tryEquipment.name}.");
            kobold.photonView.RPC(nameof(KoboldInventory.PickupEquipmentRPC), RpcTarget.All,
                EquipmentDatabase.GetID(tryEquipment), -1);
            return;
        }

        if (args[1] == "None") {
            kobold.photonView.RPC(nameof(Kobold.SetDickRPC), RpcTarget.All, byte.MaxValue);
            output.Append($"Removed dick by modifying Kobold genes.");
        }

        throw new CheatsProcessor.CommandException($"There is no equipment with name {args[1]}.");
    }
}
