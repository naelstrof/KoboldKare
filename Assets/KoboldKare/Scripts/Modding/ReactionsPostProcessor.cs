using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
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
    
    private AsyncOperationHandle<IList<ScriptableReagentReaction>> inherentAssetsHandle;
    public override async Task Awake() {
        await base.Awake();
        addedReactions = new ();
        opHandles = new();
        inherentAssetsHandle = Addressables.LoadAssetsAsync<ScriptableReagentReaction>(searchLabel.RuntimeKey, LoadReactionInherent);
        await inherentAssetsHandle.Task;
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
    
    public override async Task HandleAssetBundleMod(ModManager.ModInfoData data, AssetBundle assetBundle) {
        var key = searchLabel.labelString;
        List<Task> tasks = new List<Task>();
        var rootNode = data.assets;
        if (rootNode.HasKey(key)) {
            currentStub = new ModManager.ModStub(data);
            var array = rootNode[key].AsArray;
            foreach (var node in array) {
                if (!node.Value.IsString) continue;
                var assetName = node.Value;
                var handle = assetBundle.LoadAssetAsync<ScriptableReagentReaction>(assetName);
                handle.completed += (a) => {
                    LoadReaction(((AssetBundleRequest)a).asset as ScriptableReagentReaction);
                };
                tasks.Add(handle.AsSingleAssetTask<ScriptableReagentReaction>());
            }
        }
        await Task.WhenAll(tasks);
    }
    
    private void LoadReactionInherent(ScriptableReagentReaction reaction) {
        ReagentDatabase.AddReagentReaction(reaction, null);
    }

    private void LoadReaction(ScriptableReagentReaction reaction) {
        if (reaction == null) {
            return;
        }
        addedReactions.Add(new ModStubReactionPair() {
            stub = currentStub,
            obj = reaction
        });
        ReagentDatabase.AddReagentReaction(reaction, currentStub);
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedReactions.Count;i++) {
            if (!addedReactions[i].stub.GetRepresentedBy(data)) continue;
            ReagentDatabase.RemoveReagentReaction(addedReactions[i].obj, addedReactions[i].stub);
            addedReactions.RemoveAt(i);
            i--;
            foreach (var inherentObj in inherentAssetsHandle.Result) {
                if (addedReactions[i].obj.name != inherentObj.name) {
                    continue;
                }
                LoadReactionInherent(inherentObj);
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
