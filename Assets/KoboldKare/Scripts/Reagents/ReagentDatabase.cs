using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReagentDatabase : Database<ScriptableReagent> {
    public static void DoReactions(GenericReagentContainer container, ScriptableReagent introducedReactant) {
        ReactionsDatabase.DoReactions(container, (byte)GetID(introducedReactant));
    }
}
