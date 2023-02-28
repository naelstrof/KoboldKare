using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using System.Reflection;

public class GeneUtilities
{

    public static (List<string>, List<string>, Type koboldType) getTheGenes()
    {
        List<string> geneList = new List<string>();
        List<string> geneTypes = new List<string>();
        Type koboldType = typeof(KoboldGenes);
        FieldInfo[] koboldgeneFields = koboldType.GetFields();
        for (int i = 0; i < koboldgeneFields.Length; i++)
        {
            geneList.Add(koboldgeneFields[i].Name);
            geneTypes.Add(koboldgeneFields[i].FieldType.FullName.ToString());
        }
        return (geneList, geneTypes, koboldType);
    }

    public static List<string> geneList = GeneUtilities.getTheGenes().Item1;
    public static List<string> geneTypes = GeneUtilities.getTheGenes().Item2;
    public static Type geneType = GeneUtilities.getTheGenes().koboldType;

}


[System.Serializable]
public class CommandGene : Command
{
    public override string GetArg0() => ("/gene");


    public override void Execute(StringBuilder output, Kobold k, string[] args)
    {
        base.Execute(output, k, args);

        if (!CheatsProcessor.GetCheatsEnabled())
        {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        if ((args.Length < 2 | args.Length > 3) || (args[1] != "list" && (args.Length == 2 || !int.TryParse(args[2], out _))))
        {
            throw new CheatsProcessor.CommandException("Usage: /gene <gene> <numeric value> ; /gene list.");
        }

        if (args[1] == "list")
        {
            output.Append("Supported genes are: ");
            foreach (string gene in GeneUtilities.geneList)
            {
                if (gene != GeneUtilities.geneList[^1])
                {
                    output.Append($"{gene}" + ", ");
                }
                else
                {
                    output.Append("and " + $"{gene}" + ".\n");
                    return;
                }

            }
        }

        for (int c = 0; c < GeneUtilities.geneList.Count; c++)
        {
            if (GeneUtilities.geneList.Contains(args[1]))
            {
                if (String.Equals(args[1], GeneUtilities.geneList[c], StringComparison.CurrentCultureIgnoreCase))
                {
                    k.photonView.RequestOwnership();
                    KoboldGenes genes = k.GetGenes();

                    switch (GeneUtilities.geneList[c])
                    {
                        case "dickEquip":
                            throw new CheatsProcessor.CommandException("dickEquip is not supported through /gene. Use /dick instead.");
                        case "maxEnergy":
                            {
                                var value = float.Parse(args[2]);
                                genes.maxEnergy = value;
                                k.SetGenes(genes);
                                k.photonView.RPC(nameof(Kobold.SetEnergyRPC), RpcTarget.All, value);
                                break;
                            }
                        default:
                            if (GeneUtilities.geneTypes[c] == "System.Byte")
                            {
                                var value = byte.Parse(args[2]);
                                FieldInfo info = GeneUtilities.geneType.GetField(GeneUtilities.geneList[c]);
                                info.SetValue(genes, value);
                                break;

                            }
                            else if (GeneUtilities.geneTypes[c] == "System.Single")
                            {
                                var value = float.Parse(args[2]);
                                FieldInfo info = GeneUtilities.geneType.GetField(GeneUtilities.geneList[c]);
                                info.SetValue(genes, value);
                            }
                            else
                            {
                                throw new CheatsProcessor.CommandException("\"" + args[1] + "\"" + " did not have a valid type. Please report this, as it means a new gene type was added.");
                            }
                            break;
                    }
                    output.Append("Set " + args[1] + " to " + args[2] + "\n");
                    return;
                }
            }
            else
            {
                throw new CheatsProcessor.CommandException("\"" + args[1] + "\"" + " was not a valid gene. Use /gene list to get a full list.");
            }
        }

    }

}
