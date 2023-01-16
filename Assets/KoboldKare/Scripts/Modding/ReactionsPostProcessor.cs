using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ReactionsPostProcessor : ModPostProcessor {
    private List<ScriptableReagentReaction> addedReactions;

    public override void Awake() {
        base.Awake();
        addedReactions = new List<ScriptableReagentReaction>();
    }

    public override IEnumerator LoadAllAssets(IList<IResourceLocation> locations) {
        addedReactions.Clear();
        var opHandle = Addressables.LoadAssetsAsync<ScriptableReagentReaction>(locations, LoadReaction);
        yield return opHandle;
    }

    private void LoadReaction(ScriptableReagentReaction reaction) {
        ReagentDatabase.AddReagentReaction(reaction);
        addedReactions.Add(reaction);
    }

    public override void UnloadAllAssets(IList<IResourceLocation> locations) {
        foreach (var reaction in addedReactions) {
            ReagentDatabase.RemoveReagentReaction(reaction);
        }
        addedReactions.Clear();
    }
}
