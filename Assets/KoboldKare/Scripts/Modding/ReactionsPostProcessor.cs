using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ReactionsPostProcessor : ModPostProcessor {
    private List<ScriptableReagentReaction> addedReactions;

    public override void Awake() {
        base.Awake();
        addedReactions = new List<ScriptableReagentReaction>();
    }

    public override async Task LoadAllAssets(IList<IResourceLocation> locations) {
        addedReactions.Clear();
        var opHandle = Addressables.LoadAssetsAsync<ScriptableReagentReaction>(locations, LoadReaction);
        await opHandle.Task;
    }

    private void LoadReaction(ScriptableReagentReaction reaction) {
        if (reaction == null) {
            return;
        }
        ReagentDatabase.AddReagentReaction(reaction);
        addedReactions.Add(reaction);
    }

    public override void UnloadAllAssets() {
        foreach (var reaction in addedReactions) {
            ReagentDatabase.RemoveReagentReaction(reaction);
        }
        addedReactions.Clear();
    }
}
