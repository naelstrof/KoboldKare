using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ShopPackPostProcessor : ModPostProcessor
{   
    [SerializeField]
    private ShopDatabaseDatabase shopDatabaseDatabase;

    public virtual void Awake() {
    }
    public override AssetLabelReference GetSearchLabel() => searchLabel;

    public override async Task LoadAllAssets(IList<IResourceLocation> locations) {
        var opHandle = Addressables.LoadAssetsAsync<ShopItemPack>(locations, AddPack);
        await opHandle.Task;
    }
    private void AddPack(ShopItemPack pack){
        shopDatabaseDatabase.AddPack(pack);
    }
    public virtual void UnloadAllAssets() {
    }
}
