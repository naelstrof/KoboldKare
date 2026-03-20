using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FishNet.Connection;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandHelp : Command {
    public override string GetArg0() => "/help";
    public override void Execute(StringBuilder output, NetworkConnection conn, string[] args) {
        base.Execute(output, conn, args);
        foreach (var command in CheatsProcessor.GetCommands()) {
            output.Append($"{command.GetArg0()}\n");
            if (command.GetDescription() != null && !command.GetDescription().IsEmpty) {
                output.Append($"\t{command.GetDescription().GetLocalizedString()}\n");
            }
        }
    }
}
