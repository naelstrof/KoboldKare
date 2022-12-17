using System.Collections.Generic;
using System.Linq;
using PenetrationTech;
using UnityEngine;
using UnityEngine.Localization;

[System.Serializable]
public class DildoStuffingObjective : ObjectiveWithSpaceBeam {
    [SerializeField]
    private LocalizedString description;
    [SerializeField]
    private int minDildosInserted = 3;

    private Dictionary<Kobold, HashSet<Dildo>> penetrationMemory;

    private void AddInsertion(Kobold k, Dildo d) {
        if (!penetrationMemory.ContainsKey(k)) {
            penetrationMemory.Add(k, new HashSet<Dildo>());
        }
        penetrationMemory[k].Add(d);
        penetrationMemory[k].RemoveWhere((o) => o == null);
        TriggerUpdate();
        if (penetrationMemory[k].Count >= minDildosInserted) {
            ObjectiveManager.NetworkAdvance(k.transform.position, k.photonView.ToString());
        }
    }

    private void RemoveInsertion(Kobold k, Dildo d) {
        if (!penetrationMemory.ContainsKey(k)) {
            penetrationMemory.Add(k, new HashSet<Dildo>());
        }
        penetrationMemory[k].Remove(d);
        TriggerUpdate();
    }

    public override void Register() {
        penetrationMemory = new Dictionary<Kobold, HashSet<Dildo>>();
        base.Register();
        Dildo.dildoPenetrateStart += OnDildoPenetrateStart;
        Dildo.dildoPenetrateEnd += OnDildoPenetrateEnd;
    }
    
    public override void Unregister() {
        base.Unregister();
        Dildo.dildoPenetrateStart -= OnDildoPenetrateStart;
        Dildo.dildoPenetrateEnd -= OnDildoPenetrateEnd;
        penetrationMemory.Clear();
    }

    void OnDildoPenetrateStart(Penetrator penetrator, Penetrable penetrable) {
        Dildo d = penetrator.GetComponentInParent<Dildo>();
        Kobold k = penetrable.GetComponentInParent<Kobold>();
        if (d == null || k == null) {
            return;
        }
        AddInsertion(k, d);
    }

    void OnDildoPenetrateEnd(Penetrator penetrator, Penetrable penetrable) {
        Dildo d = penetrator.GetComponentInParent<Dildo>();
        Kobold k = penetrable.GetComponentInParent<Kobold>();
        if (d == null || k == null) {
            return;
        }

        RemoveInsertion(k, d);
    }

    public override void Advance(Vector3 position) {
        base.Advance(position);
        TriggerComplete();
    }

    public override string GetTitle() {
        int maxInsertedDildos = 0;
        if (penetrationMemory == null) {
            penetrationMemory = new Dictionary<Kobold, HashSet<Dildo>>();
        }
        foreach (var pair in penetrationMemory) {
            maxInsertedDildos = Mathf.Max(pair.Value.Count, maxInsertedDildos);
        }
        return $"{title.GetLocalizedString()} {maxInsertedDildos.ToString()}/{minDildosInserted.ToString()}";
    }

    public override string GetTextBody() {
        return description.GetLocalizedString();
    }
}
