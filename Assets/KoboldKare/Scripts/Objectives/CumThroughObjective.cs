using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KoboldKare;
using PenetrationTech;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class CumThroughObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    
    public override void Register() {
        base.Register();
        DickDescriptor.cumThrough += OnCumThrough;
    }
    
    public override void Unregister() {
        base.Unregister();
        DickDescriptor.cumThrough -= OnCumThrough;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    private void OnCumThrough(Penetrable genes) {
        Kobold kobold = genes.GetComponentInParent<Kobold>();
        if (kobold != null) {
            ObjectiveManager.NetworkAdvance(kobold.transform.position, kobold.photonView.ViewID.ToString());
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
