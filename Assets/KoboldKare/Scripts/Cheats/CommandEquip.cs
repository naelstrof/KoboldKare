using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        if (!EquipmentDatabase.TryGetAsset(args[1], out var tryEquipment)) {
            throw new CheatsProcessor.CommandException($"Equipment with name {args[1]} not found.");
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

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if (argumentIndex != 1) {
            yield break;
        }

        var assets = EquipmentDatabase.GetAssetKeys();

        foreach (var key in assets) {
            if (key.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                yield return new(key);
            }
        }

        yield return new("None", "None");
    }
}
