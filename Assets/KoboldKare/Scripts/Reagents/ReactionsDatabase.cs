using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReactionsDatabase : Database<ScriptableReagentReaction> {
    public static void DoReactions(GenericReagentContainer container, byte introducedReactant) {
        foreach(var pair in instance.assets) {
            pair.value[^1].obj.DoReaction(container);
        }
    }
}
