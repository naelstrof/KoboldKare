using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class FinalObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;
    public override void Register() {
        base.Register();
        KoboldDelivery.spawnedKobold += OnSpawnedKobold;
    }
    public override void Unregister() {
        base.Unregister();
        KoboldDelivery.spawnedKobold -= OnSpawnedKobold;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    void OnSpawnedKobold(Kobold a) {
        ObjectiveManager.NetworkAdvance(a.transform.position, $"{a.photonView.ViewID}");
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()}";
    }
}
