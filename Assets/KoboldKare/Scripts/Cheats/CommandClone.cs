using System.Collections.Generic;
using System.Text;
using System;
using NetStack.Serialization;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandClone : Command {
    [SerializeField]
    private PhotonGameObjectReference koboldPrefab;

    private static readonly string[] arg1 = new string[]
    {
        "self",
        "target",
    };

    public override string GetArg0() => "/clone";
    public override void Execute(StringBuilder output, Kobold caller, string[] args) {
        base.Execute(output, caller, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        if (args.Length < 2) {
            throw new CheatsProcessor.CommandException("Usage: /clone {self,target}");
        }

        string targetType = args[1];
        if (targetType != "self" && targetType != "target") throw new CheatsProcessor.CommandException("Usage: /clone {self,target}");

        Kobold refKobold = targetType == "self" ? caller : GetAimedAtKobold(caller);
        if (refKobold == null) throw new CheatsProcessor.CommandException("Need to be facing the kobold you want to target.");

        var callerTransform = caller.hip.transform;
        string koboldName = koboldPrefab.photonName;
        BitBuffer playerSpawnData = new(16);
        playerSpawnData.AddKoboldGenes(refKobold.GetGenes());
        playerSpawnData.AddBool(false);
        PhotonNetwork.InstantiateRoomObject(koboldName, callerTransform.position + callerTransform.forward, Quaternion.identity, 0, new object[] { playerSpawnData });
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

    public override void OnValidate() {
        base.OnValidate();
        koboldPrefab.OnValidate();
    }
}
