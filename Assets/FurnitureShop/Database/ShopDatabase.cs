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
    public List<ShopCategory> subCategories;
    public List<ShopItem> items;
    

    public async Task  Setup(){
            root=new ShopCategory();
            subCategories=new List<ShopCategory>();
            items =new List<ShopItem>();
            root.subCategories=subCategories;
            root.items=items;
            foreach(ShopItemPack itemPack in ItemPacksToLoad){
                LoadItemPack(itemPack);
            }
            foreach(ShopItem item in itemsToLoad){
                LoadItem(item);
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
                LoadItemPack(pack);
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
                LoadItem(item);
            }
            }
    //         public async Task LoadAdressables()
    // {           
    //             await LoadCategories();
    //             //await LoadFurniture();
    // }
    //  private async Task LoadCategories(){
    //         var gottenCategories = await Addressables.LoadResourceLocationsAsync(adressableCategoryTag, typeof(ShopCategory)).Task;
    //         List<Task<ShopCategory>> tasks = new List<Task<ShopCategory>>();
    //         foreach (var gottenCategory in gottenCategories)
    //         {
    //         tasks.Add(Addressables.LoadAssetAsync<ShopCategory>(gottenCategory).Task);
    //         }

    //         var loadedCategories = await Task.WhenAll(tasks);

    //         foreach (ShopCategory category in loadedCategories)
    //         {  
    //             Debug.Log("adding "+category.categoryName);
    //         MergeToList(subCategories,category);
    //         }
    // }

    private void LoadItem(ShopItem item){
            var currentCategory=root;
            foreach(string pathPiece in GetPath(item)){
                currentCategory=GetOrMakeSubCategory(currentCategory,pathPiece);
            }
            AddToCategory(currentCategory,item);
    }
    private void LoadItemPack(ShopItemPack itemPack){
            var currentCategory=root;
            foreach(string pathPiece in GetPath(itemPack)){
                currentCategory=GetOrMakeSubCategory(currentCategory,pathPiece);
            }
            AddToCategory(currentCategory,itemPack);
    }
    private ShopCategory GetOrMakeSubCategory(ShopCategory parent,string name){
        foreach(ShopCategory category in parent.subCategories){
            if(category.categoryName==name){
                return category;
            }
        }
        ShopCategory temp=MakeNewCategory(name);
        parent.subCategories.Add(temp);
        return temp;
    }
    private string[] GetPath(ShopItem item){
        return item.path.Split('/'); // '/' to make it look like url, maybe '\\' to make it look like file path? 
    }
    private string[] GetPath(ShopItemPack item){
    return item.path.Split('/');
    }
    private void AddToCategory(ShopCategory category, ShopItem item){
        category.items.Add(item);
    }

    private void AddToCategory(ShopCategory category, ShopItemPack items){
        foreach(ShopItem item in items.items)
            category.items.Add(item);
    }

    private ShopCategory MakeNewCategory(string name){
        ShopCategory newCategory=new ShopCategory();
        newCategory.categoryName=name;
        newCategory.subCategories=new List<ShopCategory>();
        newCategory.items=new List<ShopItem>();
        return newCategory;
    }
}
