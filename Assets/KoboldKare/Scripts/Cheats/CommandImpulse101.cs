using System.Text;
using FishNet.Connection;
using UnityEngine;

[System.Serializable]
public class CommandImpulse101 : Command {
    public override string GetArg0() => "/impulse101";
    public override void Execute(StringBuilder output, NetworkConnection conn, string[] args) {
        base.Execute(output, conn, args);
        if (args.Length != 1) {
            throw new CheatsProcessor.CommandException("Usage: /impulse101");
        }
        if (!CheatsProcessor.GetCheatsEnabled()) {
            throw new CheatsProcessor.CommandException("Cheats are not enabled, use `/cheats 1` to enable cheats.");
        }
        
        // FIXME FISHNET

        /*k.photonView.RequestOwnership();
        float maxValue = 99999f;
        KoboldGenes genes = k.GetGenes();
        genes.maxEnergy = maxValue; 
        genes.bellySize = maxValue;
        genes.metabolizeCapacitySize = maxValue;
        k.SetGenes(genes);
        k.photonView.RPC(nameof(Kobold.SetEnergyRPC), RpcTarget.All, maxValue/2f);*/
        output.Append("Maximized stats.\n");
    }
}
