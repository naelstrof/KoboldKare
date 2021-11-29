using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReagentDatabase : MonoBehaviour {
    private static ReagentDatabase instance;
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
    }
    public static ScriptableReagent GetReagent(string name) {
        foreach(var reagent in GetInstance().reagents) {
            if (reagent.name == name) {
                return reagent;
            }
        }
        return null;
    }
    public static ScriptableReagent GetReagent(short id) {
        return GetInstance().reagents[id];
    }
    public static short GetID(ScriptableReagent reagent) {
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
