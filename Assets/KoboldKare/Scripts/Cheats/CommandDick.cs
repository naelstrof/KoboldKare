using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;

public class DickUtilities
{
    public static (PrefabDatabase,List<PrefabDatabase.PrefabReferenceInfo>) getTheDicks()
    {
        var penisDatabase = GameManager.GetPenisDatabase();
        var penises = penisDatabase.GetValidPrefabReferenceInfos();
        return (penisDatabase,penises);
    }

    public static PrefabDatabase penisDatabase = getTheDicks().Item1;
    public static List<PrefabDatabase.PrefabReferenceInfo> penises = getTheDicks().Item2;

}

[System.Serializable]
public class CommandDick : Command
{
    public override string GetArg0() => "/dick";
    public override void Execute(StringBuilder output, Kobold k, string[] args)
    {
        base.Execute(output, k, args);
        if (!CheatsProcessor.GetCheatsEnabled())
        {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        if (args.Length != 2)
        {
            throw new CheatsProcessor.CommandException("Usage: /dick <index or name> ; /dick list.");
        }
        // List command
        if (String.Equals(args[1], "list", StringComparison.CurrentCultureIgnoreCase))
        {
            output.Append("Available dicks are: ");
            for (int i = 0; i < DickUtilities.penises.Count; i++)
            {
                if (DickUtilities.penises[i] == DickUtilities.penises[^1])
                {
                    output.Append("and " + DickUtilities.penises[i].GetKey() + ".\n");
                    return;
                }
                else
                {
                    output.Append(DickUtilities.penises[i].GetKey() + ", ");
                }
            }
        }
        // Dick setting
        if (int.TryParse(args[1], out int intValue))
        {
            if (DickUtilities.penises.Count - 1 >= intValue & intValue > -1)
            {
                k.photonView.RequestOwnership();
                k.SetDickRPC(dickID: byte.Parse(args[1]));
                output.AppendLine("Set dick to " + DickUtilities.penises[intValue].GetKey() + ".");
                return;
            }
            throw new CheatsProcessor.CommandException("Please supply a valid index or dick name. Use /dick list to list names.");
        }
        else if (!DickUtilities.penisDatabase.GetInfoByName(args[1]).IsUnityNull())
        {
            var selectedPenis = DickUtilities.penisDatabase.GetInfoByName(args[1]);
            byte byteValue = (byte)DickUtilities.penises.IndexOf(selectedPenis);
            k.photonView.RequestOwnership();
            k.SetDickRPC(dickID: byteValue);
            output.AppendLine("Set dick to " + args[1] + ".");
            return;
        }
        else
        {
            throw new CheatsProcessor.CommandException("Please supply a valid index or dick name. Use /dick list to list names.");
        }

    }

}
