using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[System.Serializable]
public class CommandSwap : Command {
    private const float DefaultDistance = 5.0f;
    public override string GetArg0() => "/swap";
    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        float distance = DefaultDistance;
        if (args.Length == 0) {
            throw new CheatsProcessor.CommandException("Usage: /swap [camera] [distance]\n" + 
                "camera: swap from the camera's point of view. If omitted, swaps from the kobold's point of view instead.\n" +
                $"distance: max distance to check. If omitted, defaults to {DefaultDistance} units");
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

        if (args.Length > 1) {
            if (float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) {
                distance = d;

                if(distance <= 0) {
                    distance = 1;
                }
            } else if (string.Equals(args[1], "camera", System.StringComparison.OrdinalIgnoreCase)) {
                aimPosition = Camera.main.transform.position;
                aimDir = Camera.main.transform.forward;
            }
        }

        if(args.Length > 2) {
            if (float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) {
                distance = d;

                if (distance <= 0) {
                    distance = 1;
                }
            }
        }

        foreach (RaycastHit hit in Physics.RaycastAll(aimPosition, aimDir, distance)) {
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

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text)
    {
        if(argumentIndex != 1) {
            yield break;
        }

        if("camera".Contains(text, System.StringComparison.OrdinalIgnoreCase)) {
            yield return new("camera (optional)", "camera");
        }
    }
}
