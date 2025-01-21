using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ShopPostProcessor : ModPostProcessor
{   [SerializeField]
    private ShopDatabaseDatabase shopDatabaseDatabase;

    public override void Awake(){
        base.Awake();
        shopDatabaseDatabase.Setup();
    }

    
    public AssetLabelReference GetSearchLabel() => searchLabel;

    public override async Task LoadAllAssets(IList<IResourceLocation> locations) {
        var opHandle = Addressables.LoadAssetsAsync<ShopDatabase>(locations, AddDatabase);
        await opHandle.Task;
    }

    private void AddDatabase(ShopDatabase database){
        if (database==null) return;
        shopDatabaseDatabase.AddDatabase(database);
    }
    public override void UnloadAllAssets() {
        shopDatabaseDatabase.Setup();
    }
}
