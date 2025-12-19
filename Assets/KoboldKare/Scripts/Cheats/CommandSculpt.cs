using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[System.Serializable]
public class CommandSculpt : Command
{
    private static readonly string[] arg1 = new string[]
    {
        "self",
        "target",
    };

    private static readonly string[] arg2 = new string[]
    {
        "balls",
        "bellycapacity",
        "boobs",
        "brightness",
        "dick",
        "dickthickness",
        "energy",
        "fat",
        "foodcapacity",
        "height",
        "hue",
        "impregnate",
        "saturation",
    };

    public override string GetArg0() => "/sculpt";

    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);

        static void Usage() {
            throw new CheatsProcessor.CommandException("Usage: /sculpt {self,target} {dick,balls,boobs,height,fat,foodcapacity,bellycapacity,dickthickness,energy,clothinghue,hue,brightness,saturation,impregnate} [set] {modifier} (set is optional)");
        }

        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length != 4 && args.Length != 5) {
            Usage();
        }

        var targetType = args[1];
        var part = args[2];
        bool set = false;
        var modifier = 0.0f;

        if (args[3].ToLowerInvariant() == "set") {
            if(args.Length != 5) {
                Usage();
            }

            set = true;

            if (float.TryParse(args[4], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out modifier) == false) {
                throw new CheatsProcessor.CommandException($"Invalid modifier: {args[4]}");
            }
        } else if(args.Length != 4) {
            Usage();
        } else {
            if (float.TryParse(args[3], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out modifier) == false) {
                throw new CheatsProcessor.CommandException($"Invalid modifier: {args[3]}");
            }
        }

        NetworkedKobold target = null;

        if (targetType == "self") {
            target = k.GetComponentInParent<NetworkedKobold>();
        } else if (targetType == "target") {
            Vector3 aimPosition = k.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Head).position;
            Vector3 aimDir = k.GetComponentInChildren<NetworkedKobold>(true).GetEyeDir();

            foreach (RaycastHit hit in Physics.RaycastAll(aimPosition, aimDir, 5f)) {
                NetworkedKobold b = hit.collider.GetComponentInParent<NetworkedKobold>();

                if (b == null) continue;
                if (b == k) continue;

                target = b;

                break;
            }

            if(target == null) {
                throw new CheatsProcessor.CommandException("Need to be facing the kobold you want to target with.");
            }

            // FIXME FISHNET
            
            /*foreach (Player player in PhotonNetwork.PlayerList) {
                if ((Kobold)player.TagObject == target && ((Kobold)PhotonNetwork.MasterClient.TagObject != k)) {
                    throw new CheatsProcessor.CommandException("Not the owner, not allowed to modify players.");
                }
            }*/
        } else {
            Usage();
        }

        if (target == null) {
            throw new CheatsProcessor.CommandException("No valid target.");
        }

        // FIXME FISHNET
        // target.photonView.RequestOwnership();

        byte SafeModify(byte value, float modifier) {
            var outValue = (int)(value + modifier);

            if(set) {
                outValue = (int)modifier;
            }

            if (outValue < 0) {
                return 0;
            }
            else if (outValue > 255) {
                return 255;
            }

            return (byte)outValue;
        }

        float ApplyFloat(float baseValue) {
            var outValue = baseValue + modifier;

            if(set) {
                outValue = modifier;
            }

            return Mathf.Max(outValue, 0.1f);
        }

        switch(part.ToLowerInvariant()) {
            case "dick":
                target.SetDickSize(ApplyFloat(target.dickSize.Value));
                break;

            case "balls":
                target.SetBallSize(ApplyFloat(target.dickSize.Value));
                break;
            case "boobs":
                target.SetBreastSize(ApplyFloat(target.breastSize.Value));
                break;
            case "height":
                target.SetBaseSize(ApplyFloat(target.baseSize.Value));
                break;
            case "fat":
                target.SetFatSize(ApplyFloat(target.fatSize.Value));
                break;
            case "bellycapacity":
                target.SetBellySize(ApplyFloat(target.bellySize.Value));
                break;
            case "foodcapacity":
                target.SetMetabolizeCapacitySize(ApplyFloat(target.metabolizeCapacitySize.Value));
                break;
            case "energy":
                target.SetMaxEnergy(ApplyFloat(target.maxEnergy.Value));
                break;
            case "dickthickness":
                target.SetDickThickness(ApplyFloat(target.dickThickness.Value));
                break;
            case "hue":
                target.SetHue(SafeModify(target.hue.Value, modifier));
                break;
            case "clothinghue":
                target.SetClothingHue(SafeModify(target.clothingHue.Value, modifier));
                break;
            case "brightness":
                target.SetBrightness(SafeModify(target.brightness.Value, modifier));
                break;
            case "saturation":
                target.SetSaturation(SafeModify(target.saturation.Value, modifier));
                break;
            case "impregnate": {
                    ReagentContents alloc = new ReagentContents();
                    if (ReagentDatabase.TryGetAsset("Cum", out var cumReagent)) {
                        alloc.AddMix(cumReagent.GetReagent(Mathf.Abs(modifier)));
                    }
                    // FIXME FISHNET
                    //target.bellyContainer.AddMix(alloc, GenericReagentContainer.InjectType.Inject);
                }
                break;
            default:
                throw new CheatsProcessor.CommandException($"Invalid body part specified: {args[2]}");
        }
    }

    public override IEnumerable<AutocompleteResult> Autocomplete(int argumentIndex, string[] arguments, string text) {
        if (!CheatsProcessor.GetCheatsEnabled()) {
            yield break;
        }
        switch(argumentIndex) {
            case 1:
                foreach(var arg in arg1) {
                    if (arg.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                        yield return new(arg);
                    }
                }
                break;
            case 2:
                foreach (var arg in arg2) {
                    if (arg.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                        yield return new(arg);
                    }
                }
                break;
        }
    }
}
