using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandSkip : Command {
    public override string GetArg0() => "/skip";

    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        switch (args.Length) {
            case 1:
                output.Append("Skipped objective.\n");
                ObjectiveManager.SkipObjective();
                return;
            case 2: {
                if (!int.TryParse(args[1], out int skipCount)) {
                    throw new CheatsProcessor.CommandException("Usage: /skip [int]");
                }

                for (int i = 0; i < skipCount; i++) {
                    ObjectiveManager.SkipObjective();
                }

                output.Append($"Skipped {skipCount.ToString()} objectives.\n");
                return;
            }
            default:
                throw new CheatsProcessor.CommandException("Usage: /skip [int]");
        }
    }
}
