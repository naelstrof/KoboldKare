using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class ExpandFarmObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;

    [SerializeField]
    private MonoBehaviour thinker;
    
    public override void Register() {
        bool found = false;
        foreach (var tile in Object.FindObjectsOfType<SoilTile>()) {
            if (!tile.GetDebris()) continue;
            spaceBeamTarget = tile.transform;
            found = true;
            break;
        }
        base.Register();
        SoilTile.tileCleared += OnTileClear;
        if (!found) {
            thinker.StartCoroutine(WaitThenClear());
        }
    }

    private IEnumerator WaitThenClear() {
        yield return null;
        TriggerComplete();
    }

    public override void Unregister() {
        base.Unregister();
        SoilTile.tileCleared -= OnTileClear;
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    private void OnTileClear(SoilTile tile) {
        ObjectiveManager.NetworkAdvance(tile.transform.position, tile.photonView.ViewID.ToString());
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} 0/1";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
