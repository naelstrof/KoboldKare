using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class DeliverFatKoboldObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    private int koboldCount = 0;
    [SerializeField]
    private int maxKobolds = 5;
    
    public override void Register() {
        DumpsterDoor.soldObject += OnSoldObject;
    }
    
    public override void Unregister() {
        DumpsterDoor.soldObject -= OnSoldObject;
    }
    
    private void OnSoldObject(GameObject obj, float worth) {
        Kobold k = obj.GetComponentInParent<Kobold>();
        if (k == null) {
            return;
        }

        if (k.GetGenes().fatSize > 20f) {
            koboldCount++;
            TriggerUpdate();
        }

        if (koboldCount >= maxKobolds) {
            TriggerComplete();
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {koboldCount.ToString()}/{maxKobolds.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
