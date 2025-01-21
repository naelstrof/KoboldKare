using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
[CreateAssetMenu(fileName = "New Shop Database Database", menuName = "Data/Shop Database Database")]
public class ShopDatabaseDatabase: ScriptableObject
{

    private List<ShopDatabase> shopDatabaseDatabase;


    public void AddDatabase(ShopDatabase database){
        database.Setup();
        shopDatabaseDatabase.Add(database);
    }
    public void AddPack(ShopItemPack pack){
        foreach (ShopDatabase database in shopDatabaseDatabase){
            database.LoadItemPack(pack);
        }
    }
    public void AddItem(ShopItem item){
        foreach (ShopDatabase database in shopDatabaseDatabase){
            database.LoadItem(item);
        }
    }
    public void Setup(){
            shopDatabaseDatabase=new List<ShopDatabase>();
    }

}
