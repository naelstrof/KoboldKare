using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class InflateKoboldObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    
    public override void Register() {
        base.Register();
        GeneHolder.containerInflated += OnInflatedEvent;
    }
    
    public override void Unregister() {
        base.Unregister();
        GeneHolder.containerInflated -= OnInflatedEvent;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    private void OnInflatedEvent(GeneHolder container) {
        // FIXME FISHNET
        /*if (container.maxVolume > 20f && container.TryGetComponent(out Kobold kobold)) {
            ObjectiveManager.NetworkAdvance(kobold.transform.position, $"{kobold.photonView.ViewID.ToString()}");
        }*/
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
