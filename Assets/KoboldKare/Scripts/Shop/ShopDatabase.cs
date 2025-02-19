using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
[CreateAssetMenu(fileName = "New Shop Database", menuName = "Data/Shop Database")]
public class ShopDatabase : ScriptableObject
{   
    
    [SerializeField]
    private List<ShopItemPack> ItemPacksToLoad;
    [SerializeField]
    private List<ShopItem> itemsToLoad;
    
    [SerializeField]
    private bool shouldLoadAdressable =false;
    [SerializeField]
    private string adressableItemPackTag;
    [SerializeField]
    private string adressableItemTag;

    private ShopCategory root;
    
    public List<ShopCategory> GetRootCategories()
    {
        return root.GetSubcategories();
    }
    public List<ShopItem> GetRootItems(){
        return root.GetItems();
    }

    public async Task  Setup(){
            root=new ShopCategory("root");
            foreach(ShopItemPack itemPack in ItemPacksToLoad){
                LoadItemPack(itemPack,root);
            }
            foreach(ShopItem item in itemsToLoad){
                LoadItem(item,root);
            }

            if(shouldLoadAdressable){
                await LoadItemPacks();
                await LoadItems();
            }
        }

        private async Task LoadItemPacks(){
            var gottenPacks = await Addressables.LoadResourceLocationsAsync(adressableItemPackTag, typeof(ShopItemPack)).Task;
            List<Task<ShopItemPack>> tasks = new List<Task<ShopItemPack>>();
            foreach (var gottenPack in gottenPacks)
            {
                tasks.Add(Addressables.LoadAssetAsync<ShopItemPack>(gottenPack).Task);
            }

            var loadedPacks = await Task.WhenAll(tasks);

            foreach (ShopItemPack pack in loadedPacks)
            {  
                LoadItemPack(pack,root);
            }
            }
        private async Task LoadItems(){
            var gottenItems = await Addressables.LoadResourceLocationsAsync(adressableItemTag, typeof(ShopItem)).Task;
            List<Task<ShopItem>> tasks = new List<Task<ShopItem>>();
            foreach (var gottenItem in gottenItems)
            {
                tasks.Add(Addressables.LoadAssetAsync<ShopItem>(gottenItem).Task);
            }

            var loadedItems = await Task.WhenAll(tasks);

            foreach (ShopItem item in loadedItems)
            {  
                LoadItem(item,root);
            }
            }


    private void LoadItem(ShopItem item,ShopCategory startingCategory){
            var currentCategory=startingCategory;
            foreach(string pathPiece in item.GetPath()){
                currentCategory=GetOrMakeSubCategory(currentCategory,pathPiece);
            }
            AddToCategory(item,currentCategory);
    }

    private void LoadItemPack(ShopItemPack itemPack,ShopCategory startingCategory){
            var currentCategory=startingCategory;
            foreach(string pathPiece in itemPack.GetPath()){
                currentCategory=GetOrMakeSubCategory(currentCategory,pathPiece);
            }
            foreach(ShopItem item in itemPack.GetItems()){
                LoadItem(item,currentCategory);
            }
    }

    private ShopCategory GetOrMakeSubCategory(ShopCategory parent,string name){
        if(name.Length<1){return parent;}
        foreach(ShopCategory category in parent.GetSubcategories()){
            if(category.GetName()==name){
                return category;
            }
        }
        ShopCategory temp=new ShopCategory(name);
        parent.GetSubcategories().Add(temp);
        return temp;
    }

    private void AddToCategory(ShopItem item,ShopCategory category){
        category.GetItems().Add(item);
    }

}
