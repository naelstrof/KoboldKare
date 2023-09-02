using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ReagentPostProcessor : ModPostProcessor {
    private List<ScriptableReagent> addedReagents;
    public override async Task LoadAllAssets(IList<IResourceLocation> locations)  {
        addedReagents.Clear();
        var opHandle = Addressables.LoadAssetsAsync<ScriptableReagent>(locations, LoadReagent);
        await opHandle.Task;
    }

    public override void Awake() {
        base.Awake();
        addedReagents = new List<ScriptableReagent>();
    }

    private void LoadReagent(ScriptableReagent reagent) {
        if (reagent == null) {
            return;
        }

        addedReagents.Add(reagent);
        ReagentDatabase.AddReagent(reagent);
    }

    public override void UnloadAllAssets() {
        foreach (var reagent in addedReagents) {
            ReagentDatabase.RemoveReagent(reagent);
        }
    }
}
