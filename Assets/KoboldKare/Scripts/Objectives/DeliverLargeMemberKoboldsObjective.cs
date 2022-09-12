using System.Collections;
using System.Collections.Generic;
using KoboldKare;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;

public class DeliverLargeMemberKoboldsObjective : DragonMailObjective {
    [SerializeField]
    private LocalizedString description;
    private int koboldCount = 0;
    [SerializeField]
    private int maxKobolds = 5;

    [SerializeField] private GameEventPhotonView soldGameObjectEvent;
    
    public override void Register() {
        soldGameObjectEvent.AddListener(OnSoldObject);
    }
    
    public override void Unregister() {
        soldGameObjectEvent.RemoveListener(OnSoldObject);
    }
    
    private void OnSoldObject(PhotonView view) {
        Kobold k = view.GetComponent<Kobold>();
        if (k == null) {
            return;
        }

        if (k.GetGenes().dickSize >= 15f) {
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
