using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
[CreateAssetMenu(fileName = "New Shop Database", menuName = "Data/Shop Database")]
public class ShopDatabase : ScriptableObject
{   
    
    [SerializeField]
    private List<ShopCategory> categoriesToLoad;
    [SerializeField]
    private List<ShopItem> itemsToLoad;
    
    [SerializeField]
    private bool shouldLoadAdressable =false;
    [SerializeField]
    private string adressableCategoryTag;
    [SerializeField]
    private string adressableItemTag;
    //private DatabaseRoot database;

    //private class DatabaseRoot{
    public List<ShopCategory> subCategories;
    public List<ShopItem> items;
    //}

    public async Task  Setup(){
            //database=new DatabaseRoot();
            subCategories=new List<ShopCategory>();
            items =new List<ShopItem>();
            foreach(ShopCategory category in categoriesToLoad){
                subCategories.Add(category);
            }
            foreach(ShopItem item in itemsToLoad){
                items.Add(item);
            }

        }


    void Merge (ShopCategory main,ShopCategory toAdd){
        foreach(ShopCategory added in toAdd.subCategories){
                foreach(ShopCategory mainCategory in main.subCategories){
                    bool needToCreate=true;
                    if(added.categoryName==mainCategory.categoryName)
                        {
                            Merge(mainCategory,added);
                            needToCreate=false;
                        }
                    if(needToCreate){
                        main.subCategories.Add(toAdd);
                    
                        }
                }
        }
        foreach(ShopItem itemToAdd in toAdd.items){
            main.items.Add(itemToAdd);
        }
        }

    
}
