using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using UnityEngine;

[System.Serializable]
public class CommandList : Command {
    public override string GetArg0() => "/list";
    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        bool didSomething = false;
        if (args.Length == 1 || args[1] == "prefabs" || args[1] == "objects") {
            DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
            if (pool == null) {
                throw new CheatsProcessor.CommandException("Failed to find PhotonNetwork pool, are you online??");
            }

            output.Append("Objects = {");
            foreach (var keyValuePair in pool.ResourceCache) {
                output.Append($"{keyValuePair.Key},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "dicks") {
            output.Append("Dicks = {");
            foreach (var info in GameManager.GetPenisDatabase().GetValidPrefabReferenceInfos()) {
                output.Append($"{info.GetKey()},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "reagents") {
            output.Append("Reagents = {");
            foreach (var reagent in ReagentDatabase.GetReagents()) {
                output.Append($"{reagent.name},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "equipment") {
            output.Append("Equipment = {");
            foreach (var equipment in EquipmentDatabase.GetEquipments()) {
                output.Append($"{equipment.name},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "players") {
            output.Append("Players = {\n");
            foreach (var player in PhotonNetwork.PlayerList) {
                output.Append($"{player.ActorNumber} {player.NickName},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }

        if (!didSomething) {
            throw new CheatsProcessor.CommandException("Usage: /list {prefabs,objects,reagents,dicks}");
        }
    }
}
