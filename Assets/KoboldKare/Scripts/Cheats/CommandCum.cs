using Photon.Pun;
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
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        if (args.Length > 2 || (args.Length != 1 && !int.TryParse(args[1], out _)))
        {
            throw new CheatsProcessor.CommandException("Usage: /cum <ballSize> ; /cum");
        }

        if (args.Length == 2)
        {
            k.photonView.RequestOwnership();
            KoboldGenes genes = k.GetGenes();
            float initialBallSize = genes.ballSize;
            k.SetGenes(genes.With(ballSize: float.Parse(args[1])));
            k.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
            k.SetGenes(genes.With(ballSize: initialBallSize));
        }
        else
        {
            k.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
        }

    }
}

