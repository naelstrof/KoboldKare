using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class DeliverThePerfectKobold : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;
    [SerializeField] private GameEventPhotonView soldGameObjectEvent;
    
    public override void Register() {
        base.Register();
        soldGameObjectEvent.AddListener(OnSoldObject);
    }
    
    public override void Unregister() {
        base.Unregister();
        soldGameObjectEvent.RemoveListener(OnSoldObject);
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    private void OnSoldObject(PhotonView view) {
        NetworkedKobold k = view.GetComponentInParent<NetworkedKobold>();
        if (k == null) {
            return;
        }

        if (!k) {
            return;
        }
        float sum = 0f;
        sum += k.baseSize.Value;
        sum += k.fatSize.Value;
        sum += k.ballSize.Value;
        sum += k.bellySize.Value;
        sum += k.dickSize.Value;
        sum += k.dickThickness.Value;
        sum += k.maxEnergy.Value;
        sum += k.fatSize.Value;
        // 150 would be the maximum value of a kobold generated randomly. This means that a value of 200 would be at least 50 units of metabolized something.
        // 210 would be at least 3 generations of fluid intake.
        if (sum > 210f) {
            // FIXME FISHNET
            /* ObjectiveManager.NetworkAdvance(spaceBeamTarget.position, view.ViewID.ToString()); */
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} 0/1";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
