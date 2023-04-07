using System;
using System.Text;
using Photon.Pun;

[Serializable]
public class CommandDick : Command {
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
        if (byte.TryParse(args[1], out byte dickID)) {
            if (dickID != byte.MaxValue && dickID >= infos.Count) {
                throw new CheatsProcessor.CommandException($"Index is invalid, must be either {byte.MaxValue} or under {infos.Count-1}.");
            }
            k.photonView.RPC(nameof(Kobold.SetDickRPC), RpcTarget.All, dickID);
            output.AppendLine("Set dick to " + infos[dickID].GetKey() + ".");
            return;
        }
        for (byte i=0;i<infos.Count;i++) {
            if (infos[i].GetKey() != args[1]) continue;
            k.photonView.RPC(nameof(Kobold.SetDickRPC), RpcTarget.All, i);
            output.AppendLine("Set dick to " + args[1] + ".");
            return;
        }
        throw new CheatsProcessor.CommandException($"Couldn't find dick with name {args[1]}.");
    }

}
