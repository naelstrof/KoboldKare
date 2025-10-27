using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ReactionsPostProcessor : ModPostProcessor {
    private struct ModStubReactionPair {
        public ModManager.ModStub stub;
        public ScriptableReagentReaction obj;
    }
    
    private List<ModStubReactionPair> addedReactions;
    
    private List<ModStubAddressableHandlePair> opHandles;

    private ModManager.ModStub currentStub;
    public override void Awake() {
        base.Awake();
        addedReactions = new ();
        opHandles = new();
    }
    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        if (locator.Locate(searchLabel.RuntimeKey, typeof(ScriptableReagentReaction), out var locations)) {
            currentStub = new ModManager.ModStub(data);
            var opHandle = Addressables.LoadAssetsAsync<ScriptableReagentReaction>(locations, LoadReaction);
            await opHandle.Task;
            opHandles.Add(new ModStubAddressableHandlePair() {
                stub = currentStub,
                handle = opHandle
            });
        }
    }

    private void LoadReaction(ScriptableReagentReaction reaction) {
        if (reaction == null) {
            return;
        }
        addedReactions.Add(new ModStubReactionPair() {
            stub = currentStub,
            obj = reaction
        });
        ReagentDatabase.AddReagentReaction(reaction);
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedReactions.Count;i++) {
            if (!addedReactions[i].stub.GetRepresentedBy(data)) continue;
            ReagentDatabase.RemoveReagentReaction(addedReactions[i].obj);
            addedReactions.RemoveAt(i);
            i--;
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
