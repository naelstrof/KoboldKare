using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StringTablePostProcessor : ModPostProcessor {
    private struct ModStubStringTablePair {
        public ModManager.ModStub stub;
        public StringTable table;
        public string primaryKey;
    }
    
    private struct ModStubSharedTableDataPair {
        public ModManager.ModStub stub;
        public SharedTableData table;
        public string primaryKey;
    }
    
    private List<ModStubStringTablePair> addedStringTables;
    private List<ModStubSharedTableDataPair> addedSharedTableData;
    
    private ModManager.ModStub currentStub;
    
    private GenericProvider<StringTable> stringProvider;
    private GenericProvider<SharedTableData> sharedTableDataProvider;
    
    private AsyncOperationHandle<IList<ScriptableReagentReaction>> inherentAssetsHandle;
    public override async Task Awake() {
        await base.Awake();
        addedStringTables = new ();
        addedSharedTableData = new();

        stringProvider = new ("KoboldKare.StringTableProvider");
        sharedTableDataProvider = new("KoboldKare.SharedTableDataProvider");
        
        Addressables.ResourceManager.ResourceProviders.Insert(0,stringProvider);
        Addressables.AddResourceLocator(new GenericResourceLocator<StringTable>("KoboldKare.StringTableLocator", stringProvider));
        
        Addressables.ResourceManager.ResourceProviders.Insert(0,sharedTableDataProvider);
        Addressables.AddResourceLocator(new GenericResourceLocator<SharedTableData>("KoboldKare.SharedTableDataLocator", sharedTableDataProvider));
    }
    public override async Task HandleAddressableMod(ModManager.ModInfoData data, IResourceLocator locator) {
        // ignore addressable mods
    }
    
    public override async Task HandleAssetBundleMod(ModManager.ModInfoData data, AssetBundle assetBundle) {
        var key = "StringTables";
        List<Task> tasks = new List<Task>();
        var rootNode = data.assets;
        if (rootNode.HasKey(key)) {
            currentStub = new ModManager.ModStub(data);
            var array = rootNode[key];
            foreach (var node in array) {
                if (!node.Value.IsString) continue;
                var assetName = node.Value;
                var handle = assetBundle.LoadAssetAsync<StringTable>(assetName);
                handle.completed += (a) => {
                    LoadStringTable(((AssetBundleRequest)a).asset as StringTable);
                };
                tasks.Add(handle.AsSingleAssetTask<StringTable>());
            }
        }
        var otherKey = "SharedStringData";
        if (rootNode.HasKey(otherKey)) {
            currentStub = new ModManager.ModStub(data);
            var array = rootNode[otherKey];
            foreach (var node in array) {
                if (!node.Value.IsString) continue;
                var assetName = node.Value;
                var primaryKey = node.Key;
                var handle = assetBundle.LoadAssetAsync<SharedTableData>(assetName);
                handle.completed += (a) => {
                    LoadSharedTableData(primaryKey, ((AssetBundleRequest)a).asset as SharedTableData);
                };
                tasks.Add(handle.AsSingleAssetTask<SharedTableData>());
            }
        }
        await Task.WhenAll(tasks);
    }

    private void LoadStringTable(StringTable table) {
        if (table == null) {
            return;
        }
        addedStringTables.Add(new ModStubStringTablePair() {
            stub = currentStub,
            table = table,
            primaryKey = table.name,
        });
        stringProvider.Add(table.name, table);
    }
    
    private void LoadSharedTableData(string primaryKey, SharedTableData table) {
        if (table == null) {
            return;
        }
        addedSharedTableData.Add(new ModStubSharedTableDataPair() {
            stub = currentStub,
            table = table,
            primaryKey = primaryKey,
        });
        sharedTableDataProvider.Add(primaryKey, table);
    }

    public override Task UnloadAssets(ModManager.ModInfoData data) {
        for (int i=0;i<addedStringTables.Count;i++) {
            if (!addedStringTables[i].stub.GetRepresentedBy(data)) continue;
            stringProvider.Remove(addedStringTables[i].primaryKey, addedStringTables[i].table);
            addedStringTables.RemoveAt(i);
            i--;
        }
        return base.UnloadAssets(data);
    }
}
