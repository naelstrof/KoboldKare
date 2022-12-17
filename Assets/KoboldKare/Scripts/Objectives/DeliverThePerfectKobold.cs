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
        Kobold k = view.GetComponent<Kobold>();
        if (k == null) {
            return;
        }

        KoboldGenes genes = k.GetGenes();
        if (genes == null) {
            return;
        }
        float sum = 0f;
        sum += genes.baseSize;
        sum += genes.fatSize;
        sum += genes.ballSize;
        sum += genes.bellySize;
        sum += genes.dickSize;
        sum += genes.dickThickness;
        sum += genes.maxEnergy;
        sum += genes.fatSize;
        // 150 would be the maximum value of a kobold generated randomly. This means that a value of 200 would be at least 50 units of metabolized something.
        // 210 would be at least 3 generations of fluid intake.
        if (sum > 210f) {
            ObjectiveManager.NetworkAdvance(spaceBeamTarget.position, view.ViewID.ToString());
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} 0/1";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
