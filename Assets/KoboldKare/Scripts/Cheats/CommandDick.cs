using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;

[Serializable]
public class CommandDick : Command {
    public const short unEquipID = 0;

    public override string GetArg0() => "/dick";

    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        if (args.Length != 2) {
            throw new CheatsProcessor.CommandException("Usage: /dick <index or name>.");
        }
        var infos = GameManager.GetPenisDatabase().GetValidPrefabReferenceInfos();
        // Dick setting
        if (short.TryParse(args[1], out short dickID)) {
            SetDickByID(output, k, infos, dickID);
        } else {
            SetDickByName(output, k, infos, args);
        }
    }

    private void SetDick(Kobold k, short dickID, StringBuilder output, string chatMessage) {
        k.photonView.RPC(nameof(Kobold.SetDickRPC), RpcTarget.All, dickID);
        output.AppendLine(chatMessage);
    }

    private void SetDickByID(StringBuilder output, Kobold k, List<PrefabDatabase.PrefabReferenceInfo> infos, short dickID) {
        if (dickID != unEquipID) {
            if (dickID < unEquipID || dickID > infos.Count) {
                throw new CheatsProcessor.CommandException($"Dick ID is invalid, must be either {unEquipID} or maximum {infos.Count}.");
            }
            SetDick(k, dickID, output, "Set dick to " + infos[dickID - 1].GetKey() + ".");
        } else {
            SetDick(k, dickID, output, "Set dick to None.");
        }
    }

    private void SetDickByName(StringBuilder output, Kobold k, List<PrefabDatabase.PrefabReferenceInfo> infos, string[] args) {
        for (short i = 0; i < infos.Count; i++) {
            if (infos[i].GetKey() != args[1]) continue;
            i++;
            SetDick(k, i, output, "Set dick to " + args[1] + ".");
            return;
        }
        throw new CheatsProcessor.CommandException($"Couldn't find dick with name {args[1]}.");
    }
}
