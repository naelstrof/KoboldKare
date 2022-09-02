using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandGive : Command {
    [SerializeField]
    private PhotonGameObjectReference bucket;
    public override string GetArg0() => "/give";
    public override void Execute(StringBuilder output, Kobold kobold, string[] args) {
        base.Execute(output, kobold, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length < 2) {
            throw new CheatsProcessor.CommandException("/give requires at least one argument. Use /list to find what you can spawn.");
        }

        DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        var koboldTransform = kobold.hip.transform;
        if (pool != null && pool.ResourceCache.ContainsKey(args[1])) {
            PhotonNetwork.InstantiateRoomObject(args[1], koboldTransform.position + koboldTransform.forward, Quaternion.identity);
            output.Append($"Spawned {args[1]}.\n");
            return;
        }
        if (ReagentDatabase.GetReagent(args[1]) != null) {
            GameObject obj = PhotonNetwork.InstantiateRoomObject(bucket.photonName, koboldTransform.position + koboldTransform.forward, Quaternion.identity);
            ReagentContents contents = new ReagentContents();
            if (args.Length > 2 && float.TryParse(args[2], out float value)) {
                contents.AddMix(ReagentDatabase.GetReagent(args[1]).GetReagent(value));
                obj.GetPhotonView().RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, contents, kobold.photonView.ViewID);
                output.Append($"Spawned bucket filled with {value} {args[1]}.\n");
                return;
            } else {
                contents.AddMix(ReagentDatabase.GetReagent(args[1]).GetReagent(20f));
                obj.GetPhotonView().RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, contents, kobold.photonView.ViewID);
                output.Append($"Spawned bucket filled with {20f} {args[1]}.\n");
            }

            throw new CheatsProcessor.CommandException($"Spawning reagents requires a 3rd number argument.");
            return;
        }

        if (args[1].ToLower() == "stars") {
            if (args.Length > 2 && int.TryParse(args[2], out int value)) {
                ObjectiveManager.GiveStars(value);
                output.Append($"Gave {value} {args[1]}.\n");
                return;
            } else {
                ObjectiveManager.GiveStars(999);
                output.Append($"Gave 999 {args[1]}.\n");
                return;
            }
        }

        if (args[1].ToLower() == "money" || args[1].ToLower() == "dosh" || args[1].ToLower() == "dollars") {
            if (args.Length > 2 && float.TryParse(args[2], out float value)) {
                kobold.photonView.RPC(nameof(MoneyHolder.AddMoney), RpcTarget.All, value);
                output.Append($"Gave {value} {args[1]} to {kobold.photonView.Owner.NickName}.\n");
                return;
            } else {
                kobold.photonView.RPC(nameof(MoneyHolder.AddMoney), RpcTarget.All, 999f);
                output.Append($"Gave 999 {args[1]} to {kobold.photonView.Owner.NickName}.\n");
                return;
            }
        }

        throw new CheatsProcessor.CommandException($"There is no prefab, reagent, or resource with name {args[1]}.");
    }

    public override void OnValidate() {
        base.OnValidate();
        bucket.OnValidate();
    }
}
