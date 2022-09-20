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
    private SoilTile[] tiles;
    
    public override void Register() {
        bool found = false;
        foreach (var tile in tiles) {
            if (!tile.GetDebris()) continue;
            spaceBeamTarget = tile.transform;
            found = true;
            break;
        }
        base.Register();
        SoilTile.tileCleared += OnTileClear;
        if (!found) {
            TriggerComplete();
        }
    }
    
    public override void Unregister() {
        base.Unregister();
        SoilTile.tileCleared -= OnTileClear;
    }

    protected override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    private void OnTileClear(SoilTile tile) {
        Advance(tile.transform.position);
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} 0/1";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
