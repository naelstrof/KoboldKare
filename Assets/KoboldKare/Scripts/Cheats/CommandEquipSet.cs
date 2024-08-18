using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Photon.Pun;
using SimpleJSON;
using Steamworks;
using UnityEngine;

[System.Serializable]
public class CommandEquipSet : Command
{
    public override string GetArg0() => "/equipset";
    public override void Execute(StringBuilder output, Kobold kobold, string[] args) {
        base.Execute(output, kobold, args);

        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        static void Usage() {
            throw new CheatsProcessor.CommandException("Usage: /equipset {create/use/add/remove/delete/list} [name] [equipment/equipment list separated by space].\nUse /list equipment to list all equipment");
        }

        var path = $"{Application.persistentDataPath}/defaultUser/equipsets.json";

        if (SteamManager.Initialized) {
            path = $"{Application.persistentDataPath}/{SteamUser.GetSteamID().ToString()}/equipsets.json";
        }

        var sets = new Dictionary<string, List<string>>();

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
        catch (System.Exception) {}

        void Save() {
            try {
                using FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
                using StreamWriter writer = new StreamWriter(file);

                JSONNode rootNode = JSONNode.Parse("{}");

                foreach(var pair in sets) {
                    var value = new JSONArray();

                    foreach(var p in pair.Value) {
                        value.Add(new JSONString(p));
                    }

                    rootNode.Add(pair.Key, value);
                }

                writer.Write(rootNode.ToString());
            } catch (System.Exception) {}
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

            case "list":

                {
                    if(sets.TryGetValue(name, out var list)) {
                        output.AppendLine($"Equipment in {name}: {string.Join(", ", list)}");
                    } else {
                        output.AppendLine($"Equip set {name} not found");
                    }
                }

                break;

            case "create":

                {
                    if(sets.TryGetValue(name, out var list) == false) {
                        list = new();

                        sets.Add(name, list);
                    } else {
                        list.Clear();
                    }

                    Save();

                    output.AppendLine($"Created or cleared equip set {name}");
                }

                break;

            case "delete":

                if(sets.ContainsKey(name)) {
                    sets.Remove(name);

                    Save();

                    output.AppendLine($"Deleted equip set {name}");
                } else {
                    output.AppendLine($"Equip set {name} not found");
                }

                break;

            case "use":

                {
                    if (sets.TryGetValue(name, out var pieces)) {
                        foreach (var piece in pieces) {
                            if (piece.Trim().Length == 0) {
                                continue;
                            }

                            try {
                                var tryEquipment = EquipmentDatabase.GetEquipment(piece);

                                kobold.photonView.RPC(nameof(KoboldInventory.PickupEquipmentRPC), RpcTarget.All,
                                    EquipmentDatabase.GetID(tryEquipment), -1);

                                output.AppendLine($"Equipped {piece}");
                            }
                            catch (UnityException e) {
                                output.AppendLine($"Equipment {piece} not equipped because it was not found");
                            }
                        }
                    } else {
                        output.AppendLine($"Equip set {name} not found!");
                    }
                }

                break;

            case "add":

                {
                    if (equipNames.Count == 0) {
                        Usage();
                    }

                    if(sets.TryGetValue(name, out var list) == false) {
                        output.AppendLine($"Equip set {name} not found!");

                        return;
                    }

                    foreach(var equipName in equipNames) {
                        if (list.Contains(equipName)) {
                            output.AppendLine($"Equip set {name} already has {equipName}!");
                        } else {
                            try {
                                EquipmentDatabase.GetEquipment(equipName);
                            } catch (UnityException) {
                                output.AppendLine($"Equipment {equipName} not found!");

                                continue;
                            }

                            list.Add(equipName);

                            output.AppendLine($"Added {equipName} to equip set {name}");
                        }
                    }

                    Save();
                }

                break;

            default:

                Usage();

                break;
        }
    }
}
