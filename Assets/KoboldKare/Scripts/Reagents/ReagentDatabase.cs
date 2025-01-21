using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReagentDatabase : MonoBehaviour {
    private static ReagentDatabase instance;
    private Dictionary<string,ScriptableReagent> reagentDictionary;
    private ScriptableReagent defaultReagent;
    private class ReagentSorter : IComparer<ScriptableReagent> {
        public int Compare(ScriptableReagent x, ScriptableReagent y) {
            return String.Compare(x.name, y.name, StringComparison.InvariantCulture);
        }
    }
    private ReagentSorter reagentSorter;
    public void Awake() {
        if (instance != null) {
            Destroy(this);
            return;
        } else {
            instance = this;
        }

        defaultReagent = ScriptableObject.CreateInstance<ScriptableReagent>();
        
        reagentSorter = new ReagentSorter();
        reagentDictionary = new Dictionary<string, ScriptableReagent>();
        foreach(var reagent in reagents) {
            reagentDictionary.Add(reagent.name, reagent);
        }

        if (reagentDictionary.Count > 255) {
            throw new UnityException("Too many reagents, only support up to 255 unique reagents...");
        }
    }
    public static ScriptableReagent GetReagent(string name) {
        if (!ModManager.GetReady()) {
            return instance.defaultReagent;
        }
        if (instance.reagentDictionary.ContainsKey(name)) {
            return instance.reagentDictionary[name];
        }

        return null;
    }
    public static ScriptableReagent GetReagent(byte id) {
        if (!ModManager.GetReady()) {
            return instance.defaultReagent;
        }
        
        return instance.reagents[id];
    }
    public static byte GetID(ScriptableReagent reagent) {
        return (byte)instance.reagents.IndexOf(reagent);
    }

    public static void AddReagent(ScriptableReagent newReagent) {
        for (int i = 0; i < instance.reagents.Count; i++) {
            var reagent = instance.reagents[i];
            // Replace strategy
            if (reagent.name == newReagent.name) {
                instance.reagents[i] = newReagent;
                instance.reagentDictionary[newReagent.name] = newReagent;
                instance.reagents.Sort(instance.reagentSorter);
                return;
            }
        }

        instance.reagents.Add(newReagent);
        instance.reagentDictionary.Add(newReagent.name, newReagent);
        instance.reagents.Sort(instance.reagentSorter);
    }
    
    public static void RemoveReagent(ScriptableReagent reagent) {
        if (instance.reagents.Contains(reagent)) {
            instance.reagents.Remove(reagent);
            instance.reagentDictionary.Remove(reagent.name);
        }
    }

    public static void AddReagentReaction(ScriptableReagentReaction newReaction) {
        for (int i = 0; i < instance.reagents.Count; i++) {
            var reaction = instance.reactions[i];
            if (reaction.name != newReaction.name) continue;
            instance.reactions[i] = newReaction;
            return;
        }

        instance.reactions.Add(newReaction);
    }
    public static void RemoveReagentReaction(ScriptableReagentReaction newReaction) {
        if (instance.reactions.Contains(newReaction)) {
            instance.reactions.Remove(newReaction);
        }
    }

    public static List<ScriptableReagent> GetReagents() => instance.reagents;
    public List<ScriptableReagent> reagents;
    public List<ScriptableReagentReaction> reactions;
    public static void DoReactions(GenericReagentContainer container, ScriptableReagent introducedReactant) {
        DoReactions(container, GetID(introducedReactant));
    }
    public static void DoReactions(GenericReagentContainer container, byte introducedReactant) {
        foreach(var reaction in instance.reactions) {
            reaction.DoReaction(container);
        }
    }
}
