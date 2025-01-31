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
    private string[] myTags;

    private ShopCategory root;
    
    public List<ShopCategory> GetRootCategories()
    {
        return root.GetSubcategories();
    }
    public List<ShopItem> GetRootItems(){
        return root.GetItems();
    }

    public void Setup(){
            root=new ShopCategory("root");
            foreach(ShopItemPack itemPack in ItemPacksToLoad){
                LoadItemPack(itemPack,root);
            }
            foreach(ShopItem item in itemsToLoad){
                LoadItem(item,root);
            }

    }
    public void LoadItemPack(ShopItemPack pack){
                if(TagCheck(pack)) 
                    {LoadItemPack(pack,root);}

    }
    public void LoadItemPacks(List<ShopItemPack> loadedPacks){

            foreach (ShopItemPack pack in loadedPacks)
            {  
                LoadItemPack(pack);
            }
    }

    private bool TagCheck(ShopItemPack pack){
            if(myTags.Length==0 || pack.GetTags().Length==0) return false;
            foreach (string tag in pack.GetTags()){
                foreach (string mytag in myTags)
                    if (tag.ToLower()==mytag.ToLower()) return true;
            }
            return false;
    }
    public void LoadItem(ShopItem item){
                if(TagCheck(item)) 
                    {LoadItem(item,root);}
    }
    public void  LoadItems(List<ShopItem> loadedItems){

            foreach (ShopItem item in loadedItems)
            {  
                LoadItem(item);
            }
    }

    private bool TagCheck(ShopItem item){
            if(myTags.Length==0 || item.GetTags().Length==0) return false;
            foreach (string tag in item.GetTags()){
                foreach (string mytag in myTags)
                    if (tag.ToLower()==mytag.ToLower()) return true;
            }
            return false;
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
