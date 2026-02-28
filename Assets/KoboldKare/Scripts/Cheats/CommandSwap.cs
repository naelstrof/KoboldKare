using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[System.Serializable]
public class CommandSwap : Command {
    public override string GetArg0() => "/swap";
    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        if (args.Length != 1) {
            throw new CheatsProcessor.CommandException("Usage: /swap");
        }
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        BrainSwapperMachine machine = Object.FindObjectOfType<BrainSwapperMachine>();
        if (machine == null) {
            throw new CheatsProcessor.CommandException("Couldn't find the brain swapper machine, its required to exist in the world in order to trigger a swap...");
        }

        Kobold b = GetAimedAtKobold(k);
        if (k == null) throw new CheatsProcessor.CommandException("Need to be facing the kobold you want to swap with.");

        Player aPlayer = null;
        Player bPlayer = null;
        foreach (Player player in PhotonNetwork.PlayerList) {
            if ((Kobold)player.TagObject == k) {
                aPlayer = player;
            }

            if ((Kobold)player.TagObject == b) {
                bPlayer = player;
            }
        }

        machine.photonView.RPC(nameof(BrainSwapperMachine.AssignKobolds), RpcTarget.All, k.photonView.ViewID,
            b.photonView.ViewID, bPlayer?.ActorNumber ?? -1, aPlayer?.ActorNumber ?? -1,
            b.GetComponent<MoneyHolder>().GetMoney(), k.GetComponent<MoneyHolder>().GetMoney());
        output.Append($"Swapped kobolds.\n");
    }
}
