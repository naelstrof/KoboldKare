using System.Text;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[System.Serializable]
public class CommandImpulse101 : Command {
    public override string GetArg0() => "/impulse101";
    public override void Execute(StringBuilder output, Kobold k, string[] args) {
        base.Execute(output, k, args);
        if (args.Length != 1) {
            throw new CheatsProcessor.CommandException("Usage: /impulse101");
        }
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }

        k.photonView.RequestOwnership();
        KoboldGenes genes = k.GetGenes();
        genes.maxEnergy = float.MaxValue;
        genes.bellySize = float.MaxValue;
        genes.metabolizeCapacitySize = float.MaxValue;
        k.SetGenes(genes);
        k.photonView.RPC(nameof(Kobold.SetEnergyRPC), RpcTarget.All, float.MaxValue);
        output.Append("Maximized stats.\n");
    }
}
