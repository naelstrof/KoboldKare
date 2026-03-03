using Photon.Realtime;
using System.Collections.Generic;
using System.Text;
using System;
using Photon.Pun;

[System.Serializable]
public class CommandFlush : Command {
    private static readonly string[] arg1 = new string[]
    {
        "self",
        "target",
    };

    public override string GetArg0() => "/flush";
    public override void Execute(StringBuilder output, Kobold caller, string[] args) {
        base.Execute(output, caller, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        if (args.Length < 2) {
            throw new CheatsProcessor.CommandException("Usage: /flush {self,target}");
        }

        string targetType = args[1];
        if (targetType != "self" && targetType != "target") throw new CheatsProcessor.CommandException("Usage: /flush {self,target}");

        Kobold target = targetType == "self" ? caller : GetAimedAtKobold(caller);
        if (target == null) throw new CheatsProcessor.CommandException("Need to be facing the kobold you want to target.");

        if (caller != target && caller != (Kobold)PhotonNetwork.MasterClient.TagObject) {
            foreach (Player player in PhotonNetwork.PlayerList) {
                if ((Kobold)player.TagObject == target) {
                    throw new CheatsProcessor.CommandException("Not the owner, not allowed to modify players.");
                }
            }
        }

        target.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.All, target.bellyContainer.volume);
    }

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if(argumentIndex != 1) {
            yield break;
        }

        foreach(var arg in arg1) {
            if (arg.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                yield return new(arg);
            }
        }
    }
}
