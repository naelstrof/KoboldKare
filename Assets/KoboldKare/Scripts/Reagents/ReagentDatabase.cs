using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReagentDatabase : Database<ScriptableReagent> {
    public static void DoReactions(GeneHolder container) {
        ReactionsDatabase.DoReactions(container);
    }
}
