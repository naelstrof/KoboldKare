using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FishNet;
using FishNet.Connection;
using NetStack.Serialization;
using Photon.Pun;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class CommandGive : Command {
    [SerializeField]
    private PhotonGameObjectReference bucket;

    public static readonly string[] parameters = new string[] {
        "stars",
        "money",
        "dosh",
        "dollars",
        "machines",
    };

    public override string GetArg0() => "/give";
    public override void Execute(StringBuilder output, NetworkConnection connection, string[] args) {
        base.Execute(output, connection, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length < 2) {
            throw new CheatsProcessor.CommandException("/give requires at least one argument. Use /list to find what you can spawn.");
        }
        if (!KoboldEntitySpawner.TryGetPlayerKobold(connection, out var networkedKobold)) {
            throw new CheatsProcessor.CommandException("Failed to find player kobold.");
        }
        if (!networkedKobold.TryGetKobold(out var kobold)) {
            throw new CheatsProcessor.CommandException("Kobold not ready yet, please wait.");
        }
        
        var koboldTransform = kobold.hip.transform;
        if (KoboldKareObjectPostProcessor.GetAssetGroupFromKey(args[1], out var group)) {
            if (group == "Reagent") {
                if (bucket == null || !bucket.TryGetAssetGroupAndKey(out var bucketGroup, out var bucketKey)) {
                    throw new CheatsProcessor.CommandException("Failed to spawn bucket, bucket reference is not valid.");
                }

                if (!ReagentDatabase.TryGetAsset(args[1], out var check)) {
                    throw new CheatsProcessor.CommandException($"Failed to get reagent with name {args[1]}");
                }
                var data = KoboldEntitySpawner.NetworkedEntityInstantiationData.Default();
                data.groupName = bucketGroup;
                data.assetName = bucketKey;
                data.position = kobold.transform.position + kobold.transform.forward * 1f;
                data.rotation = kobold.transform.rotation;
                data.reagentContents.maxVolume = 20f;
                if (args.Length > 2 && float.TryParse(args[2], out float value)) {
                    data.reagentContents.maxVolume = value;
                    data.reagentContents.AddMix(check.GetReagent(value));
                } else {
                    data.reagentContents.AddMix(check.GetReagent(20f));
                }

                InstanceFinder.NetworkManager.GetComponent<KoboldEntitySpawner>().SpawnAsServer(data, true, kobold.GetComponentInParent<NetworkedKobold>().Owner);
                output.Append($"Spawned bucket filled with {20f} {args[1]}.\n");
                return;
                
            }
        }

        // FIXME FISHNET
        /*DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
        var koboldTransform = kobold.hip.transform;
        if (pool != null && pool.ResourceCache.ContainsKey(args[1])) {
            PhotonNetwork.Instantiate(args[1], koboldTransform.position + koboldTransform.forward, Quaternion.identity);
            output.Append($"Spawned {args[1]}.\n");
            return;
        }
        if (ReagentDatabase.TryGetAsset(args[1], out var check)) {
            GameObject obj = PhotonNetwork.Instantiate(bucket.photonName, koboldTransform.position + koboldTransform.forward, Quaternion.identity);
            ReagentContents contents = new ReagentContents();
            if (args.Length > 2 && float.TryParse(args[2], out float value)) {
                contents.AddMix(check.GetReagent(value));
                BitBuffer buffer = new BitBuffer(4);
                buffer.AddReagentContents(contents);
                obj.GetPhotonView().RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, buffer, kobold.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
                output.Append($"Spawned bucket filled with {value} {args[1]}.\n");
                return;
            }
            contents.AddMix(check.GetReagent(20f));
            BitBuffer otherBuffer = new BitBuffer(4);
            otherBuffer.AddReagentContents(contents);
            obj.GetPhotonView().RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, otherBuffer, kobold.photonView.ViewID, (byte)GenericReagentContainer.InjectType.Inject);
            output.Append($"Spawned bucket filled with {20f} {args[1]}.\n");
            return;
        }

        if (args[1].ToLower() == "stars") {
            if (args.Length > 2 && int.TryParse(args[2], out int value)) {
                ObjectiveManager.GiveStars(value);
                output.Append($"Gave {value} {args[1]}.\n");
                return;
            }
            ObjectiveManager.GiveStars(999);
            output.Append($"Gave 999 {args[1]}.\n");
            return;
        }

        if (args[1].ToLower() == "money" || args[1].ToLower() == "dosh" || args[1].ToLower() == "dollars") {
            if (args.Length > 2 && float.TryParse(args[2], out float value)) {
                kobold.photonView.RPC(nameof(MoneyHolder.AddMoney), RpcTarget.All, value);
                output.Append($"Gave {value} {args[1]} to {kobold.photonView.Owner.NickName}.\n");
                return;
            }
            kobold.photonView.RPC(nameof(MoneyHolder.AddMoney), RpcTarget.All, 999f);
            output.Append($"Gave 999 {args[1]} to {kobold.photonView.Owner.NickName}.\n");
            return;
        }
        
        if (args[1].ToLower() == "machines") {
            var allContracts = Object.FindObjectsOfType<MachineConstructionContract>();
            foreach(var curContract in allContracts) {
                curContract.ForceState(true);
            }
            output.Append($"Constructed all {allContracts.Length} machines.\n");
            return;
        }*/

        throw new CheatsProcessor.CommandException($"There is no prefab, reagent, or resource with name {args[1]}.");
    }

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if(argumentIndex != 1) {
            yield break;
        }

        // FIXME FISHNET
        /*if (PhotonNetwork.PrefabPool is DefaultPool pool) {
            foreach (var pair in pool.ResourceCache) {
                if(pair.Key.Contains(text, System.StringComparison.OrdinalIgnoreCase)) {
                    yield return new(pair.Key);
                }
            }
        }*/

        foreach (var key in ReagentDatabase.GetAssetKeys()) {
            if (key.Contains(text, System.StringComparison.OrdinalIgnoreCase)) {
                yield return new(key);
            }
        }

        foreach(var parameter in parameters) {
            if(parameter.Contains(text, System.StringComparison.OrdinalIgnoreCase)) {
                yield return new(parameter);
            }
        }
    }
}
