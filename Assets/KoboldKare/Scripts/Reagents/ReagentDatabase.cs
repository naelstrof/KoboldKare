using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReagentDatabase : MonoBehaviour {
    private static ReagentDatabase instance;
    private ScriptableReagent defaultReagent;
    private class ReagentSorter : IComparer<string> {
        public int Compare(string x, string y) {
            return String.Compare(x, y, StringComparison.InvariantCulture);
        }
    }

    public static List<ScriptableReagent> GetReagents() {
        List<ScriptableReagent> reagents = new();
        foreach (var pair in instance.reagents) {
            reagents.Add(pair.Value[^1].reagent);
        }
        return reagents;
    }
    
    private SortedDictionary<string,List<ReagentStubPair>> reagents = new(new ReagentSorter());
    private SortedDictionary<string,List<ReactionStubPair>> reactions = new(new ReagentSorter());
    private struct ReagentStubPair {
        public ModManager.ModStub? stub;
        public ScriptableReagent reagent;
        public bool GetRepresentedByStub(ModManager.ModStub? b) {
            if (b == null && stub == null) {
                return true;
            }
            if (b == null || stub == null) {
                return false;
            }
            return stub.Value.GetRepresentedBy(b.Value);
        }
    }
    private struct ReactionStubPair {
        public ModManager.ModStub? stub;
        public ScriptableReagentReaction reaction;
        public bool GetRepresentedByStub(ModManager.ModStub? b) {
            if (b == null && stub == null) {
                return true;
            }
            if (b == null || stub == null) {
                return false;
            }
            return stub.Value.GetRepresentedBy(b.Value);
        }
    }
    private static int CompareReagentStubPair(ReagentStubPair x, ReagentStubPair y) {
        if (x.stub == null && y.stub == null) return 0;
        if (x.stub == null) return -1;
        if (y.stub == null) return 1;
        if (x.stub.Value.loadPriority == y.stub.Value.loadPriority) {
            return String.Compare(x.stub.Value.title, y.stub.Value.title, StringComparison.InvariantCulture);
        }
        return y.stub.Value.loadPriority.CompareTo(x.stub.Value.loadPriority);
    }
    private static int CompareReactionStubPair(ReactionStubPair x, ReactionStubPair y) {
        if (x.stub == null && y.stub == null) return 0;
        if (x.stub == null) return -1;
        if (y.stub == null) return 1;
        if (x.stub.Value.loadPriority == y.stub.Value.loadPriority) {
            return String.Compare(x.stub.Value.title, y.stub.Value.title, StringComparison.InvariantCulture);
        }
        return x.stub.Value.loadPriority.CompareTo(y.stub.Value.loadPriority);
    }
    
    public void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        } else {
            instance = this;
        }

        defaultReagent = ScriptableObject.CreateInstance<ScriptableReagent>();
    }
    public static ScriptableReagent GetReagent(string name) {
        if (!ModManager.GetReady()) {
            return instance.defaultReagent;
        }

        if (instance.reagents.TryGetValue(name, out var reagentPair)) {
            return reagentPair[^1].reagent;
        }

        if (instance.reagents.Count > 0) {
            return instance.reagents.ElementAt(0).Value[^1].reagent;
        } else {
            return instance.defaultReagent;
        }
    }
    
    public static ScriptableReagent GetReagent(byte id) {
        if (!ModManager.GetReady()) {
            return instance.defaultReagent;
        }

        if (id >= instance.reagents.Count) {
#if UNITY_EDITOR
            Debug.LogError("Tried to access invalid reagent ID: " + id);
#endif
            return instance.defaultReagent;
        }
        return instance.reagents.ElementAt(id).Value[^1].reagent;
    }
    public static byte GetID(ScriptableReagent reagent) {
        for(int i=0;i<instance.reagents.Count;i++) {
            var reagentList = instance.reagents.ElementAt(i).Value;
            if (reagentList[^1].reagent == reagent) {
                return (byte)i;
            }
        }
        return (byte)0;
    }

    public static void AddReagent(ScriptableReagent newReagent, ModManager.ModStub? stub) {
        var newKey = newReagent.name;
        if (!instance.reagents.ContainsKey(newKey)) {
            instance.reagents.Add(newKey, new());
        }
        var list = instance.reagents[newKey];
        list.Add(new ReagentStubPair() {
            reagent = newReagent,
            stub = stub
        });
        list.Sort(CompareReagentStubPair);
    }
    
    public static void RemoveReagent(ScriptableReagent reagent, ModManager.ModStub? stub) {
        var key = reagent.name;
        if (!instance.reagents.TryGetValue(key, out var list)) {
            return;
        }
        for (int i = 0; i < list.Count; i++) {
            if (list[i].GetRepresentedByStub(stub)) {
                list.RemoveAt(i);
                i--;
            }
        }
        if (list.Count == 0) {
            instance.reagents.Remove(key);
        }
        list.Sort(CompareReagentStubPair);
    }

    public static void AddReagentReaction(ScriptableReagentReaction newReaction, ModManager.ModStub? stub) {
        var newKey = newReaction.name;
        if (!instance.reactions.ContainsKey(newKey)) {
            instance.reactions.Add(newKey, new());
        }
        var list = instance.reactions[newKey];
        list.Add(new ReactionStubPair() {
            reaction = newReaction,
            stub = stub
        });
        list.Sort(CompareReactionStubPair);
    }
    public static void RemoveReagentReaction(ScriptableReagentReaction newReaction, ModManager.ModStub? stub) {
        var key = newReaction.name;
        if (!instance.reactions.TryGetValue(key, out var list)) {
            return;
        }
        for (int i = 0; i < list.Count; i++) {
            if (list[i].GetRepresentedByStub(stub)) {
                list.RemoveAt(i);
                i--;
            }
        }
        if (list.Count == 0) {
            instance.reactions.Remove(key);
        }
        list.Sort(CompareReactionStubPair);
    }
    public static void DoReactions(GenericReagentContainer container, ScriptableReagent introducedReactant) {
        DoReactions(container, GetID(introducedReactant));
    }
    public static void DoReactions(GenericReagentContainer container, byte introducedReactant) {
        foreach(var pair in instance.reactions) {
            pair.Value[^1].reaction.DoReaction(container);
        }
    }
}
