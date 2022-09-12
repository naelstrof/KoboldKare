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
public class MakePartsBiggerObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;

    private int partsCount = 0;
    [SerializeField]
    private int maxPartsBigger = 6;
    
    public override void Register() {
        throw new NotImplementedException();
        //genesChangedEvent.AddListener(OnGenesChangedEvent);
    }
    
    public override void Unregister() {
        throw new NotImplementedException();
        //genesChangedEvent.RemoveListener(OnGenesChangedEvent);
    }
    
    private void OnGenesChangedEvent(KoboldGenes genes) {
        TriggerUpdate();
        throw new NotImplementedException();
        //if (fruitCount >= maxFruit) {
            //TriggerComplete();
        //}
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
