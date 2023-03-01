using Photon.Pun;
using System.Text;

[System.Serializable]

public class CommandCum : Command
{
    public override string GetArg0() => "/cum";
    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        switch (args.Length) {
            case 1:
                k.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
                break;
            case 2: {
                k.photonView.RequestOwnership();
                KoboldGenes genes = k.GetGenes();
                float initialBallSize = genes.ballSize;
                if (!float.TryParse(args[1], out float newBallSize)) {
                    throw new CheatsProcessor.CommandException("Usage: /cum <ballSize> ; /cum");
                }
                k.SetGenes(genes.With(ballSize: newBallSize));
                k.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
                k.SetGenes(genes.With(ballSize: initialBallSize));
                break;
            }
            default: throw new CheatsProcessor.CommandException("Usage: /cum <ballSize> ; /cum");
        }
    }
}

