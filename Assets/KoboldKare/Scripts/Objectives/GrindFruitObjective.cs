using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class GrindFruitObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;

    private int fruitCount = 0;
    [SerializeField]
    private int maxFruit = 5;
    
    public override void Register() {
        GrinderManager.grindedObject += OnGrindedObject;
    }
    
    public override void Unregister() {
        GrinderManager.grindedObject -= OnGrindedObject;
    }
    
    private void OnGrindedObject(ReagentContents contents) {
        fruitCount++;
        TriggerUpdate();
        if (fruitCount >= maxFruit) {
            TriggerComplete();
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {fruitCount.ToString()}/{maxFruit.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
