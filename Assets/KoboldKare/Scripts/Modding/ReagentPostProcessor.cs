using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ReagentPostProcessor : ModPostProcessor {
    private List<ScriptableReagent> addedReagents;
    public override IEnumerator LoadAllAssets(IList<IResourceLocation> locations)  {
        addedReagents.Clear();
        var opHandle = Addressables.LoadAssetsAsync<ScriptableReagent>(locations, LoadReagent);
        yield return opHandle;
    }

    public override void Awake() {
        base.Awake();
        addedReagents = new List<ScriptableReagent>();
    }

    private void LoadReagent(ScriptableReagent reagent) {
        addedReagents.Add(reagent);
        ReagentDatabase.AddReagent(reagent);
    }

    public override void UnloadAllAssets(IList<IResourceLocation> locations) {
        foreach (var reagent in addedReagents) {
            ReagentDatabase.RemoveReagent(reagent);
        }
    }
}
