using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandHelp : Command {
    public override string GetArg0() => "/help";
    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        foreach (var command in CheatsProcessor.GetCommands()) {
            output.Append($"{command.GetArg0()}\n");
            if (command.GetDescription() != null && !command.GetDescription().IsEmpty) {
                output.Append($"\t{command.GetDescription().GetLocalizedString()}\n");
            }
        }
    }
}
