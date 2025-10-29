using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ReagentPostProcessor : ModPostProcessor {
    private struct ModStubReagentPair {
        public ModManager.ModStub stub;
        public ScriptableReagent obj;
    }
    
    private List<ModStubReagentPair> addedReagents;
    
    private List<ModStubAddressableHandlePair> opHandles;

    private ModManager.ModStub currentStub;
    
    private AsyncOperationHandle inherentAssetsHandle;
    
    public override async Task Awake() {
        await base.Awake();
        addedReagents = new ();
        opHandles = new();
        inherentAssetsHandle = Addressables.LoadAssetsAsync<ScriptableReagent>(searchLabel.RuntimeKey, LoadReagentInherent);
        await inherentAssetsHandle.Task;
    }
    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (locator.Locate(searchLabel.RuntimeKey, typeof(ScriptableReagent), out var locations)) {
            currentStub = new ModManager.ModStub(data);
            var opHandle = Addressables.LoadAssetsAsync<ScriptableReagent>(locations, LoadReagent);
            await opHandle.Task;
            opHandles.Add(new ModStubAddressableHandlePair() {
                stub = currentStub,
                handle = opHandle
            });
        }
    }
    
    private void LoadReagentInherent(ScriptableReagent reagent) {
        ReagentDatabase.AddReagent(reagent);
    }
    
    private void LoadReagent(ScriptableReagent reagent) {
        if (!reagent) {
            return;
        }
        addedReagents.Add(new ModStubReagentPair() {
            stub = currentStub,
            obj = reagent
        });
        ReagentDatabase.AddReagent(reagent);
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedReagents.Count;i++) {
            if(addedReagents[i].stub.GetRepresentedBy(data)) {
                ReagentDatabase.RemoveReagent(addedReagents[i].obj);
                addedReagents.RemoveAt(i);
                i--;
            }
        }
        for (int i=0;i<opHandles.Count;i++) {
            if(opHandles[i].stub.GetRepresentedBy(data)) {
                if (opHandles[i].handle.IsValid()) {
                    Addressables.Release(opHandles[i].handle);
                }
                opHandles.RemoveAt(i);
                i--;
            }
        }
        return base.UnloadAssets(data);
    }

}
