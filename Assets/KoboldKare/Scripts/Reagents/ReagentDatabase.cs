using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReagentDatabase : Database<ScriptableReagent> {
    public static async Task LoadReagents(List<string> names) {
        await instance.LoadAllAssets("Reagent", names);
    }
    public static void DoReactions(GenericReagentContainer container) {
        ReactionsDatabase.DoReactions(container);
    }
}
