using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class CreateFoodObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    [SerializeField]
    private List<ScriptableReagent> reagentFilter;
    [SerializeField]
    private int foodNeeded = 4;
    private int foodMade;
    
    public override void Register() {
        BucketWeapon.foodCreated += OnFoodCreatedEvent;
    }
    
    public override void Unregister() {
        BucketWeapon.foodCreated -= OnFoodCreatedEvent;
    }

    protected override void Advance(Vector3 position) {
        base.Advance(position);
        foodMade++;
        TriggerUpdate();
        if (foodMade >= foodNeeded) {
            TriggerComplete();
        }
    }

    private void OnFoodCreatedEvent(BucketWeapon bucket, ScriptableReagent reagent) {
        if (reagentFilter.Contains(reagent)) {
            Advance(bucket.transform.position);
        }
    }

    public override string GetTitle() {
        return $"{title.GetLocalizedString()} {foodMade.ToString()}/{foodNeeded.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
