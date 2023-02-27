using System.Text;

[System.Serializable]

public class CommandCum : Command
{
    public override string GetArg0() => "/cum";
    public override void Execute(StringBuilder output, Kobold k, string[] args)
    {
        base.Execute(output, k, args);
        if (!CheatsProcessor.GetCheatsEnabled())
        {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.\n");
        }

        if (args.Length < 0 | args.Length > 2) 
        {
            throw new CheatsProcessor.CommandException("Usage: /cum <ballSize> or /cum\n");
        }

        
        if (args.Length != 1 && !int.TryParse(args[1], out _)) {
            throw new CheatsProcessor.CommandException("You must supply a numeric value as the argument. /cum <ballSize>\n");
        }
        
        if (args.Length == 2)
        {
            k.photonView.RequestOwnership();
            KoboldGenes genes = k.GetGenes();
            float initialBallSize = genes.ballSize;
            float floatValue = float.Parse(args[1]);
            genes.ballSize = floatValue;
            k.SetGenes(genes);
            k.Cum();
            genes.ballSize = initialBallSize;
            k.SetGenes(genes);
        }
        else
        {
            k.Cum();
        }

    }
}

