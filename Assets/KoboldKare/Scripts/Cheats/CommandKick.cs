using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandKick : Command {
    public override string GetArg0() => "/kick";
    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        if (args.Length != 2) {
            throw new CheatsProcessor.CommandException("Usage: /kick {actor number}");
        }
        if (!int.TryParse(args[1], out int actorNum)) {
            throw new CheatsProcessor.CommandException("Must use actor number to identify player, use `/list players` to find that.");
        }
        if (k != (Kobold)PhotonNetwork.LocalPlayer.TagObject || !PhotonNetwork.IsMasterClient) {
            throw new CheatsProcessor.CommandException("Not allowed to kick players.");
        }

        foreach (var player in PhotonNetwork.PlayerList) {
            if (player.ActorNumber == actorNum && Equals(player, PhotonNetwork.LocalPlayer) && Application.isEditor) {
                NetworkManager.instance.TriggerDisconnect();
                return;
            }

            if (player.ActorNumber == actorNum && Equals(player, PhotonNetwork.LocalPlayer)) {
                throw new CheatsProcessor.CommandException("Don't kick yourself :(");
            }
            if (player.ActorNumber != actorNum) continue;
            PhotonNetwork.CloseConnection(player);
            return;
        }
        throw new CheatsProcessor.CommandException($"No player found with id {actorNum}, use `/list players`.");
    }
}
