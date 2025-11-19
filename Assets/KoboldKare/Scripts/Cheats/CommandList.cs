using System;
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

            if (PhotonNetwork.PrefabPool is not DefaultPool pool)
            {
                throw new CheatsProcessor.CommandException("Failed to find PhotonNetwork pool, are you online??");
            }

            output.Append("Objects = {\n");
            foreach (var keyValuePair in pool.ResourceCache) {
                output.Append($"{keyValuePair.Key},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "kobolds") {

            if(PhotonNetwork.PrefabPool is not DefaultPool pool)
            {
                throw new CheatsProcessor.CommandException("Failed to find PhotonNetwork pool, are you online??");
            }

            output.Append("Kobolds = {\n");
            foreach (var keyValuePair in pool.ResourceCache)
            {
                if(keyValuePair.Value.GetComponent<Kobold>() == null)
                {
                    continue;
                }

                output.Append($"{keyValuePair.Key},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "dicks") {
            output.Append("Dicks = {\n");
            foreach (var info in GameManager.GetPenisDatabase().GetValidPrefabReferenceInfos()) {
                output.Append($"{info.GetKey()},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "reagents") {
            output.Append("Reagents = {\n");
            foreach (var reagent in ReagentDatabase.GetAssets()) {
                output.Append($"{reagent.name},\n");
            }
            output.Append("}\n");
            didSomething = true;
        }
        if (args.Length == 1 || args[1] == "equipment") {
            output.Append("Equipment = {\n");
            foreach (var equipment in EquipmentDatabase.GetAssets()) {
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

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if (!CheatsProcessor.GetCheatsEnabled()) {
            yield break;
        }
        
        if(argumentIndex != 1) {
            yield break;
        }

        yield return new("prefabs");
        yield return new("objects");
        yield return new("kobolds");
        yield return new("dicks");
        yield return new("reagents");
        yield return new("equipment");
        yield return new("players");
    }
}
