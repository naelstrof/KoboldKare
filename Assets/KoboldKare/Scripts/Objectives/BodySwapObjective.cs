using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class BodySwapObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;
    public override void Register() {
        base.Register();
        BrainSwapperMachine.bodySwapped += OnBodySwap;
    }
    public override void Unregister() {
        base.Unregister();
        BrainSwapperMachine.bodySwapped -= OnBodySwap;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    void OnBodySwap(Kobold a, Kobold b) {
        foreach (var player in PhotonNetwork.PlayerList) {
            if ((Kobold)player.TagObject != a && (Kobold)player.TagObject != b) continue;
            if (a != null) {
                ObjectiveManager.NetworkAdvance(a.transform.position, $"{a.photonView.ViewID.ToString()}");
            } else if (b != null) {
                ObjectiveManager.NetworkAdvance(b.transform.position, $"{b.photonView.ViewID.ToString()}");
            }
        }
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} 0/1";
    }
}
