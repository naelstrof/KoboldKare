using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FishNet.Connection;
using UnityEngine;

[System.Serializable]
public class CommandEquip : Command {
    public override string GetArg0() => "/equip";
    public override void Execute(StringBuilder output, NetworkConnection kobold, string[] args) {
        base.Execute(output, kobold, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length < 2) {
            throw new CheatsProcessor.CommandException("/equip requires at least one argument. Use `/list equipment` to find what you can equip.");
        }

        if (args[1] == "None") {
            // FIXME FISHNET
            //kobold.photonView.RPC(nameof(Kobold.SetDickRPC), RpcTarget.All, byte.MaxValue);
            output.Append($"Removed dick by modifying Kobold genes.");
            return;
        }

        if (!KoboldKareObjectPostProcessor.HasAssetInGroup("Equipment",args[1])) {
            throw new CheatsProcessor.CommandException($"Equipment with name {args[1]} not found.");
        } else {
            output.Append($"Equipped {args[1]}.");
            // FIXME FISHNET
            // kobold.photonView.RPC(nameof(KoboldInventory.PickupEquipmentRPC), RpcTarget.All, EquipmentDatabase.GetID(tryEquipment), -1);
        }

        throw new CheatsProcessor.CommandException($"There is no equipment with name {args[1]}.");
    }

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if (!CheatsProcessor.GetCheatsEnabled()) {
            yield break;
        }
        if (argumentIndex != 1) {
            yield break;
        }

        List<string> assets = new();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup("Equipment", assets);

        foreach (var key in assets) {
            if (key.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                yield return new(key);
            }
        }

        yield return new("None", "None");
    }
}
