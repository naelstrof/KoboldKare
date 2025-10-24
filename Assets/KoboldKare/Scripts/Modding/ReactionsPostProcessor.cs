using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ReactionsPostProcessor : ModPostProcessor {
    AsyncOperationHandle opHandle;
    public override async Task LoadAllAssets() {
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        opHandle = Addressables.LoadAssetsAsync<ScriptableReagentReaction>(assetsHandle.Result, LoadReaction);
        await opHandle.Task;
        Addressables.Release(assetsHandle);
    }

    private void LoadReaction(ScriptableReagentReaction reaction) {
        if (reaction == null) {
            return;
        }
        ReagentDatabase.AddReagentReaction(reaction);
    }

    public override Task UnloadAllAssets() {
        ReagentDatabase.ClearAllReactions();
        if (opHandle.IsValid()) {
            Addressables.Release(opHandle);
        }
        return Task.CompletedTask;
    }
}
