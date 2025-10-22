using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ReagentPostProcessor : ModPostProcessor {
    AsyncOperationHandle opHandle;
    
    public override async Task LoadAllAssets()  {
        var assetsHandle = Addressables.LoadResourceLocationsAsync(searchLabel.RuntimeKey);
        await assetsHandle.Task;
        opHandle = Addressables.LoadAssetsAsync<ScriptableReagent>(assetsHandle.Result, LoadReagent);
        await opHandle.Task;
        Addressables.Release(assetsHandle);
    }
    private void LoadReagent(ScriptableReagent reagent) {
        if (!reagent) {
            return;
        }
        ReagentDatabase.AddReagent(reagent);
    }

    public override Task UnloadAllAssets() {
        ReagentDatabase.ClearAllReagents();
        if (opHandle.IsValid()) {
            Addressables.Release(opHandle);
        }
        return Task.CompletedTask;
    }
}
