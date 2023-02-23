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

        Vector3 aimPosition = k.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Head).position;
        Vector3 aimDir = k.GetComponentInChildren<CharacterControllerAnimator>(true).eyeDir;
        foreach (RaycastHit hit in Physics.RaycastAll(aimPosition, aimDir, 5f)) {
            Kobold b = hit.collider.GetComponentInParent<Kobold>();
            if (b == null) continue;
            if (b == k) continue;
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
            return;
        }
        throw new CheatsProcessor.CommandException("Need to be facing the kobold you want to swap with.");
    }
}
