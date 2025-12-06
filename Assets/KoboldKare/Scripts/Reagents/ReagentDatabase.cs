using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReagentDatabase : Database<ScriptableReagent> {
    public static void DoReactions(GenericReagentContainer container) {
        ReactionsDatabase.DoReactions(container);
    }
}
