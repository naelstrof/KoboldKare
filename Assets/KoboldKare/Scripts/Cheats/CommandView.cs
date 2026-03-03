using System;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class CommandView : Command
{
    private static readonly string[] arg1 = new string[]
    {
        "self",
        "target",
    };

    private static readonly string[] statNames = new string[]
    {
        "balls",
        "bellycapacity",
        "boobs",
        "brightness",
        "clothinghue",
        "dick",
        "dickthickness",
        "dicktype",
        "energy",
        "fat",
        "foodcapacity",
        "grabcount",
        "height",
        "hue",
        "saturation",
        "species",
    };

    private static readonly string[] multiStatNames = new string[]
    {
        "all",
        "hsv",
        "chsv",
    };

    private static string GetStatString(Kobold target, String statName) {
        return statName switch
        {
            "balls"         => string.Format("{0:f}", target.GetGenes().ballSize),
            "bellycapacity" => string.Format("{0:f}", target.GetGenes().bellySize),
            "boobs"         => string.Format("{0:f}", target.GetGenes().breastSize),
            "brightness"    => string.Format("{0:d}", target.GetGenes().brightness),
            "clothinghue"   => string.Format("{0:d}", target.GetGenes().clothingHue),
            "dick"          => string.Format("{0:f}", target.GetGenes().dickSize),
            "dickthickness" => string.Format("{0:f}", target.GetGenes().dickThickness),
            "dicktype"      => target.GetGenes().dickEquip == 0 ? CommandDick.unEquipName : GameManager.GetPenisDatabase().GetValidPrefabReferenceInfos()[target.GetGenes().dickEquip - 1].GetKey(),
            "energy"        => string.Format("{0:f}", target.GetGenes().maxEnergy),
            "fat"           => string.Format("{0:f}", target.GetGenes().fatSize),
            "foodcapacity"  => string.Format("{0:f}", target.GetGenes().metabolizeCapacitySize),
            "grabcount"     => string.Format("{0:d}", target.GetGenes().grabCount),
            "height"        => string.Format("{0:f}", target.GetGenes().baseSize),
            "hue"           => string.Format("{0:d}", target.GetGenes().hue),
            "saturation"    => string.Format("{0:d}", target.GetGenes().saturation),
            "species"       => GameManager.GetPlayerDatabase().GetValidPrefabReferenceInfos()[target.GetGenes().species].GetKey(),
            _ => throw new CheatsProcessor.CommandException($"Invalid stat specified: {statName}"),
        };
    }

    private static int statNameMaxLength = 0;

    public override string GetArg0() => "/view";

    public override void Execute(StringBuilder output, Kobold caller, string[] args) {
        base.Execute(output, caller, args);

        static void Usage() {
            throw new CheatsProcessor.CommandException("Usage: /view {self,target} {all,dick,dicktype,balls,boobs,height,fat,foodcapacity,bellycapacity,dickthickness,energy,clothinghue,hue,saturation,grabcount,species,hsv,chsv}");
        }

        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length != 3) {
            Usage();
        }

        string targetType = args[1];
        if (targetType != "self" && targetType != "target") Usage();

        Kobold target = targetType == "self" ? caller : GetAimedAtKobold(caller);
        if (target == null) throw new CheatsProcessor.CommandException("Need to be facing the kobold you want to target.");

        var statName = args[2].ToLowerInvariant();

        switch (statName) {
            case "dicktype":
                output.Append($"The dick type is currently {GetStatString(target, statName)}\n");
                break;
            case "species":
                output.Append($"The species is {GetStatString(target, statName)}\n");
                break;
            case "all":
                output.Append($"The stats are currently\n");

                // TODO: This works fine in-editor, but nothing from this for loop shows up when tested in a built game. Needs further debugging.
                foreach (string sn in statNames) {
                    int spaceCount = statNameMaxLength - sn.Length;
                    spaceCount = (int)(spaceCount * 1.8f); // The chat font isn't monospaced, have to approximate it :(
                    string padding = new(' ', spaceCount);

                    //output.Append($"  {sn,-15} {GetStatString(target, sn)}\n"); // TODO: If the chat ever switches to a monospaced font, use this instead and fix the other alignments.
                    output.Append($"  {sn}{padding}     {GetStatString(target, sn)}\n");
                }

                break;
            case "hsv":
                output.Append($"The HSV values are currently\n");
                output.Append($"  hue              {GetStatString(target, "hue")}\n");
                output.Append($"  saturation   {GetStatString(target, "saturation")}\n");
                output.Append($"  brightness  {GetStatString(target, "brightness")}\n");

                break;
            case "chsv":
                output.Append($"The CHSV values are currently\n");
                output.Append($"  clothinghue   {GetStatString(target, "clothinghue")}\n");
                output.Append($"  hue                  {GetStatString(target, "hue")}\n");
                output.Append($"  saturation      {GetStatString(target, "saturation")}\n");
                output.Append($"  brightness     {GetStatString(target, "brightness")}\n");

                break;
            default:
                output.Append($"The {statName} stat is currently {GetStatString(target, statName)}\n");
                break;
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
                foreach (var arg in statNames) {
                    if (arg.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                        yield return new(arg);
                    }
                }

                foreach (var arg in multiStatNames) {
                    if (arg.Contains(text, StringComparison.OrdinalIgnoreCase)) {
                        yield return new(arg);
                    }
                }
                break;
        }
    }

    public override void OnValidate() {
        base.OnValidate();

        foreach (var statName in statNames) {
            statNameMaxLength = Math.Max(statNameMaxLength, statName.Length);
        }
    }
}
