using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReagentDatabase : MonoBehaviour {
    private static ReagentDatabase instance;
    private Dictionary<string,ScriptableReagent> reagentDictionary;
    private static ReagentDatabase GetInstance() {
        if (instance == null) {
            instance = Object.FindObjectOfType<ReagentDatabase>();
        }
        return instance;
    }
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
        if (GetInstance().reagentDictionary.ContainsKey(name)) {
            return GetInstance().reagentDictionary[name];
        }
        return null;
    }
    public static ScriptableReagent GetReagent(short id) {
        return GetInstance().reagents[id];
    }
    public static short GetID(ScriptableReagent reagent) {
        if (GetInstance() == null) {
            return 0;
        }
        return (short)GetInstance().reagents.IndexOf(reagent);
    }
    public static List<ScriptableReagent> GetReagents() => GetInstance().reagents;
    public List<ScriptableReagent> reagents;
    public List<ScriptableReagentReaction> reactions;
    public static void DoReactions(GenericReagentContainer container, ScriptableReagent introducedReactant) {
        DoReactions(container, GetID(introducedReactant));
    }
    public static void DoReactions(GenericReagentContainer container, short introducedReactant) {
        foreach(var reaction in GetInstance().reactions) {
            reaction.DoReaction(container);
        }
    }
}
