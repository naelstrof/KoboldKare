using Photon.Pun;
using SimpleJSON;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class CommandEquipSet : Command {
    public string FilePath {
        get {
            var path = $"{Application.persistentDataPath}/defaultUser/equipsets.json";

            if (SteamManager.Initialized) {
                path = $"{Application.persistentDataPath}/{SteamUser.GetSteamID().ToString()}/equipsets.json";
            }

            return path;
        }
    }

    public static readonly string[] parameters = new string[] {
        "list",
        "create",
        "delete",
        "use",
        "add",
    };

    public override string GetArg0() => "/equipset";
    private static void Usage() {
        throw new CheatsProcessor.CommandException("Usage: /equipset {create/use/add/remove/delete/list} [name] [equipment/equipment list separated by space].\nUse /list equipment to list all equipment");
    }

    private Dictionary<string, List<string>> LoadSets() {
        var path = FilePath;
        var sets = new Dictionary<string, List<string>>();

        if (File.Exists(path)) {
            try {
                using FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);
                using StreamReader reader = new StreamReader(file);

                JSONNode rootNode = JSONNode.Parse(reader.ReadToEnd());

                foreach (var key in rootNode.Keys) {
                    var value = rootNode[key];

                    if (value != null && value is JSONArray array) {
                        var content = new List<string>();

                        foreach (var v in value.Values) {
                            if (v is JSONString str) {
                                content.Add(str.Value);
                            }
                        }

                        sets.Add(key, content);
                    }
                }
            }
            catch (System.Exception e) {
                throw new CheatsProcessor.CommandException($"Failed to read equipset.json for reason: {e.Message}");
            }
        }

        return sets;
    }

    private void Save(Dictionary<string, List<string>> sets) {
        try {
            using FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new StreamWriter(file);

            JSONNode rootNode = JSONNode.Parse("{}");

            foreach (var pair in sets) {
                var value = new JSONArray();

                foreach (var p in pair.Value)
                {
                    value.Add(new JSONString(p));
                }

                rootNode.Add(pair.Key, value);
            }

            writer.Write(rootNode.ToString());
        }
        catch (System.Exception e) {
            throw new CheatsProcessor.CommandException($"Failed to write equipset.json for reason: {e.Message}");
        }
    }

    public override void Execute(StringBuilder output, Kobold kobold, string[] args) {
        base.Execute(output, kobold, args);

        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        var path = FilePath;

        var sets = LoadSets();

        bool Add(string equipset, List<string> equipNames) {
            if (equipNames.Count == 0) {
                Usage();
            }

            if(sets.TryGetValue(equipset, out var list) == false) {
                output.AppendLine($"Equip set {equipset} not found!");
                return false;
            }

            foreach(var equipName in equipNames) {
                if (list.Contains(equipName)) {
                    output.AppendLine($"Equip set {equipset} already has {equipName}!");
                } else {
                    if (!EquipmentDatabase.TryGetAsset(equipName, out var tryEquipment)) {
                        output.AppendLine($"Equipment {equipName} not found!");
                        return false;
                    }

                    list.Add(equipName);

                    output.AppendLine($"Added {equipName} to equip set {equipset}");
                }
            }
            Save(sets);
            return true;
        }

        if (args.Length == 2 && args[1] == "list") {
            output.AppendLine($"Equip Sets: {string.Join(", ", sets.Keys)}");

            return;
        }

        if (args.Length < 3) {
            Usage();
        }

        var operation = args[1];
        var name = args.Length > 2 ? args[2] : "";
        var equipNames = new List<string>();

        if (args.Length >= 4) {
            equipNames.AddRange(args.Skip(3));
        }

        switch (operation.ToLowerInvariant()) {
            case "list": {
                if(sets.TryGetValue(name, out var list)) {
                    output.AppendLine($"Equipment in {name}: {string.Join(", ", list)}");
                } else {
                    output.AppendLine($"Equip set {name} not found");
                }
                break;
            }

            case "create": {
                if(sets.TryGetValue(name, out var list) == false) {
                    list = new();
                    sets.Add(name, list);
                } else {
                    list.Clear();
                }
                
                Add(name, equipNames);
                
                Save(sets);

                output.AppendLine($"Created or cleared equip set {name}");
                break;
            }

            case "delete":
                if(sets.ContainsKey(name)) {
                    sets.Remove(name);
                    Save(sets);
                    output.AppendLine($"Deleted equip set {name}");
                } else {
                    output.AppendLine($"Equip set {name} not found");
                }
                break;
            
            case "use": {
                if (sets.TryGetValue(name, out var pieces)) {
                    foreach (var piece in pieces) {
                        if (piece.Trim().Length == 0) {
                            continue;
                        }

                        if (EquipmentDatabase.TryGetAsset(piece, out var tryEquipment)) {
                            kobold.photonView.RPC(nameof(KoboldInventory.PickupEquipmentRPC), RpcTarget.All,
                                EquipmentDatabase.GetID(tryEquipment), -1);

                            output.AppendLine($"Equipped {piece}");
                        } else {
                            output.AppendLine($"Equipment {piece} not equipped because it was not found");
                        }
                    }
                } else {
                    output.AppendLine($"Equip set {name} not found!");
                }
                break;
            }

            case "add": {
                if (!Add(name, equipNames)) {
                    return;
                }
                break;
            }

            default:
                Usage();
                break;
        }
    }

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if (!CheatsProcessor.GetCheatsEnabled()) {
            yield break;
        }
        switch(argumentIndex) {
            case 1:
                foreach(var parameter in parameters) {
                    if(parameter.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                        yield return new(parameter);
                    }
                }

                break;

            case 2:
                var argument = arguments[1].ToLowerInvariant();
                switch(argument) {
                    case "list":
                        //No extra args
                        yield break;
                    case "create":
                        //Can't autocomplete
                        yield break;
                    case "delete":
                    case "use":
                    case "add": {
                        var sets = LoadSets();
                        foreach (var pair in sets) {
                            if(pair.Key.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                                yield return new(pair.Key);
                            }
                        }
                        break;
                    }
                }
                break;
            default:
                yield break;
        }
    }
}
