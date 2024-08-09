using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[System.Serializable]
public class CommandSculpt : Command
{
    public override string GetArg0() => "/sculpt";

    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);

        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length != 4) {
            throw new CheatsProcessor.CommandException("Usage: /sculpt {self,target} {dick,balls,boobs,height,hue,brightness,saturation} {modifier}");
        }

        Kobold target = null;

        if (args[1] == "self") {
            target = k;
        } else if (args[1] == "target") {
            Vector3 aimPosition = k.GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Head).position;
            Vector3 aimDir = k.GetComponentInChildren<CharacterControllerAnimator>(true).eyeDir;

            foreach (RaycastHit hit in Physics.RaycastAll(aimPosition, aimDir, 5f)) {
                Kobold b = hit.collider.GetComponentInParent<Kobold>();

                if (b == null) continue;
                if (b == k) continue;

                target = b;

                break;
            }

            if(target == null) {
                throw new CheatsProcessor.CommandException("Need to be facing the kobold you want to target with.");
            }

            foreach (Player player in PhotonNetwork.PlayerList) {
                if ((Kobold)player.TagObject == target && ((Kobold)PhotonNetwork.MasterClient.TagObject != k)) {
                    throw new CheatsProcessor.CommandException("Not the owner, not allowed to modify players.");
                }
            }
        } else {
            throw new CheatsProcessor.CommandException("Usage: /sculpt {self,target} {dick,balls,boobs,height,hue,brightness,saturation} {modifier}");
        }

        if(target == null) {
            throw new CheatsProcessor.CommandException("No valid target.");
        }

        if (float.TryParse(args[3], out var modifier) == false) {
            throw new CheatsProcessor.CommandException($"Invalid modifier value: {modifier}");
        }

        target.photonView.RequestOwnership();

        var genes = target.GetGenes();

        static byte SafeModify(byte value, float modifier) {
            var iValue = (int)value;

            var outValue = iValue + modifier;

            if (outValue < 0) {
                return 0;
            }
            else if (outValue > 255) {
                return 255;
            }

            return (byte)outValue;
        }

        switch(args[2].ToLowerInvariant()) {
            case "dick":

                target.SetGenes(genes.With(dickSize: genes.dickSize + modifier));

                break;

            case "balls":

                target.SetGenes(genes.With(ballSize: genes.ballSize + modifier));

                break;

            case "boobs":

                target.SetGenes(genes.With(breastSize: genes.breastSize + modifier));

                break;

            case "height":

                target.SetGenes(genes.With(baseSize: genes.baseSize + modifier));

                break;

            case "hue":

                target.SetGenes(genes.With(hue: SafeModify(genes.hue, modifier)));

                break;

            case "brightness":

                target.SetGenes(genes.With(brightness: SafeModify(genes.brightness, modifier)));

                break;

            case "saturation":

                target.SetGenes(genes.With(saturation: SafeModify(genes.saturation, modifier)));

                break;

            default:

                throw new CheatsProcessor.CommandException($"Invalid body part specified: {args[2]}");
        }
    }
}
