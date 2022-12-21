using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReagentDatabase : MonoBehaviour {
    private static ReagentDatabase instance;
    private Dictionary<string,ScriptableReagent> reagentDictionary;
    public void Awake() {
        if (instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
        reagentDictionary = new Dictionary<string, ScriptableReagent>();
        foreach(var reagent in reagents) {
            reagentDictionary.Add(reagent.name, reagent);
        }
    }
    public static ScriptableReagent GetReagent(string name) {
        if (instance.reagentDictionary.ContainsKey(name)) {
            return instance.reagentDictionary[name];
        }
        return null;
    }
    public static ScriptableReagent GetReagent(short id) {
        return instance.reagents[id];
    }
    public static short GetID(ScriptableReagent reagent) {
        if (instance == null) {
            return 0;
        }
        return (short)instance.reagents.IndexOf(reagent);
    }
    public static List<ScriptableReagent> GetReagents() => instance.reagents;
    public List<ScriptableReagent> reagents;
    public List<ScriptableReagentReaction> reactions;
    public static void DoReactions(GenericReagentContainer container, ScriptableReagent introducedReactant) {
        DoReactions(container, GetID(introducedReactant));
    }
    public static void DoReactions(GenericReagentContainer container, short introducedReactant) {
        foreach(var reaction in instance.reactions) {
            reaction.DoReaction(container);
        }
    }
}
