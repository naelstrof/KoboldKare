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

        if (reagentDictionary.Count > 255) {
            throw new UnityException("Too many reagents, only support up to 255 unique reagents...");
        }
    }
    public static ScriptableReagent GetReagent(string name) {
        if (instance.reagentDictionary.ContainsKey(name)) {
            return instance.reagentDictionary[name];
        }
        return null;
    }
    public static ScriptableReagent GetReagent(byte id) {
        return instance.reagents[id];
    }
    public static byte GetID(ScriptableReagent reagent) {
        if (instance == null) {
            return 0;
        }
        return (byte)instance.reagents.IndexOf(reagent);
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
