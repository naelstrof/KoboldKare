using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ShopItemPostProcessor : ModPostProcessor
{   
   [SerializeField]
    private ShopDatabaseDatabase shopDatabaseDatabase;

    public virtual void Awake() {
    }
    public override AssetLabelReference GetSearchLabel() => searchLabel;

    public override async Task LoadAllAssets(IList<IResourceLocation> locations) {
        var opHandle = Addressables.LoadAssetsAsync<ShopItem>(locations, AddItem);
        await opHandle.Task;
    }
    private void AddItem(ShopItem item){
        shopDatabaseDatabase.AddItem(item);
    }
    public virtual void UnloadAllAssets() {
    }
}
