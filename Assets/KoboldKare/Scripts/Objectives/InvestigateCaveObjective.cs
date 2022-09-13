using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Localization;
using Object = UnityEngine.Object;

[System.Serializable]
public class InvestigateCaveObjective : DragonMailObjective {
    [SerializeField] private Collider caveArea;
    private class KoboldDetector : MonoBehaviour {
        public delegate void KoboldEnteredZoneAction(Kobold k);
        public static KoboldEnteredZoneAction entered;
        private void OnTriggerEnter(Collider other) {
            if (other.GetComponentInParent<Kobold>() != null) {
                entered?.Invoke(other.GetComponentInParent<Kobold>());
            }
        }
    }

    private KoboldDetector detector;

    [SerializeField]
    private LocalizedString description;
    public override void Register() {
        base.Register();
        caveArea.isTrigger = true;
        detector ??= caveArea.gameObject.AddComponent<KoboldDetector>();
        KoboldDetector.entered += OnKoboldEnterZone;
    }

    public override void Unregister() {
        base.Unregister();
        KoboldDetector.entered -= OnKoboldEnterZone;
        if (detector != null) {
            Object.Destroy(detector);
        }
    }

    private void OnKoboldEnterZone(Kobold k) {
        TriggerComplete();
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
