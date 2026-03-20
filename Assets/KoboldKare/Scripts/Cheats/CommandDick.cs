using System;
using System.Collections.Generic;
using System.Text;
using FishNet.Connection;
using Photon.Pun;

[Serializable]
public class CommandDick : Command {
    public const short unEquipID = 0;

    public override string GetArg0() => "/dick";

    public override void Execute(StringBuilder output, NetworkConnection conn, string[] args) {
        base.Execute(output, conn, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        if (args.Length != 2) {
            throw new CheatsProcessor.CommandException("Usage: /dick <index or name>.");
        }

        List<string> dickNames = new List<string>();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup("Penis", dickNames);
        if (!KoboldEntitySpawner.TryGetPlayerKobold(conn, out var networkedKobold)) {
            throw new CheatsProcessor.CommandException("Failed to find player kobold.");
        }
        if (!networkedKobold.TryGetKobold(out var kobold)) {
            throw new CheatsProcessor.CommandException("Kobold not ready yet, please wait.");
        }
        if (short.TryParse(args[1], out short dickID)) {
            SetDickByID(output, kobold, dickNames, dickID);
        } else {
            SetDickByName(output, kobold, dickNames, args);
        }
    }

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if (!CheatsProcessor.GetCheatsEnabled()) {
            yield break;
        }
        if(argumentIndex != 1) {
            yield break;
        }

        List<string> dickNames = new List<string>();
        KoboldKareObjectPostProcessor.GetAllAssetNamesInGroup("Penis", dickNames);
        foreach(var info in dickNames) {
            if(info.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                yield return new(info);
            }
        }
    }

    private void SetDick(Kobold k, short dickID, StringBuilder output, string chatMessage) {
        throw new NotImplementedException();
        // FIXME: fishnet
        //k.photonView.RPC(nameof(Kobold.SetDickRPC), RpcTarget.All, dickID);
        output.AppendLine(chatMessage);
    }

    private void SetDickByID(StringBuilder output, Kobold k, List<string> infos, short dickID) {
        if (dickID != unEquipID) {
            if (dickID < unEquipID || dickID > infos.Count) {
                throw new CheatsProcessor.CommandException($"Dick ID is invalid, must be either {unEquipID} or maximum {infos.Count}.");
            }
            SetDick(k, dickID, output, "Set dick to " + infos[dickID - 1] + ".");
        } else {
            SetDick(k, dickID, output, "Set dick to None.");
        }
    }

    private void SetDickByName(StringBuilder output, Kobold k, List<string> infos, string[] args) {
        for (short i = 0; i < infos.Count; i++) {
            if (infos[i] != args[1]) continue;
            i++;
            SetDick(k, i, output, "Set dick to " + args[1] + ".");
            return;
        }
        throw new CheatsProcessor.CommandException($"Couldn't find dick with name {args[1]}.");
    }
}
