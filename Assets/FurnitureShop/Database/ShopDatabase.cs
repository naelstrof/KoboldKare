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

    public List<ShopCategory> subCategories;
    public List<ShopItem> items;
    private ShopCategory root;

    public async Task  Setup(){
            root=new ShopCategory();
            subCategories=new List<ShopCategory>();
            items =new List<ShopItem>();
            root.subCategories=subCategories;
            root.items=items;
            LoadItems(itemsToLoad);
        //     foreach(ShopCategory category in categoriesToLoad){
        //         subCategories.Add(category);
        //     }
        //     foreach(ShopItem item in itemsToLoad){
        //         items.Add(item);
        //     }
        // if(shouldLoadAdressable){
        //    await LoadAdressables();
        // }
        }
            public async Task LoadAdressables()
    {           
                await LoadCategories();
                //await LoadFurniture();
    }
     private async Task LoadCategories(){
            var gottenCategories = await Addressables.LoadResourceLocationsAsync(adressableCategoryTag, typeof(ShopCategory)).Task;
            List<Task<ShopCategory>> tasks = new List<Task<ShopCategory>>();
            foreach (var gottenCategory in gottenCategories)
            {
            tasks.Add(Addressables.LoadAssetAsync<ShopCategory>(gottenCategory).Task);
            }

            var loadedCategories = await Task.WhenAll(tasks);

            foreach (ShopCategory category in loadedCategories)
            {  
                Debug.Log("adding "+category.categoryName);
            MergeToList(subCategories,category);
            }
    }

    private void LoadItems(List<ShopItem> items){
        foreach(ShopItem item in items){
            var currentCategory=root;
            foreach(string pathPiece in GetPath(item)){
                currentCategory=GetOrMakeSubCategory(currentCategory,pathPiece);
            }
            AddToCategory(currentCategory,item);
        }

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
        return item.path.Split('/');
    }
    private void AddToCategory(ShopCategory category, ShopItem item){
        category.items.Add(item);
    }



    void MergeToList(List<ShopCategory> main,ShopCategory toAdd){
        bool needToCreate=true;
        foreach(ShopCategory mainCategory in main){
            Debug.Log("comparing "+mainCategory.categoryName+" to "+toAdd.categoryName+ " is "+(mainCategory.categoryName==toAdd.categoryName).ToString());
            if (mainCategory.categoryName==toAdd.categoryName){
                needToCreate=false;
                Merge(mainCategory,toAdd);
                break;
            }
        }
        
        if(needToCreate){
            Debug.Log("making list"+toAdd.categoryName);
            ShopCategory temp=MakeNewCategory(toAdd.categoryName);
            main.Add(temp);
            Merge(temp,toAdd);
            }
    }

    void Merge (ShopCategory main,ShopCategory toAdd){
        Debug.Log("Merging"+main.categoryName+" and "+toAdd.categoryName);
        foreach(ShopCategory added in toAdd.subCategories){
            bool needToCreate=true;
                foreach(ShopCategory mainCategory in main.subCategories){
                    if(added.categoryName==mainCategory.categoryName)
                        {
                        Merge(mainCategory,added);
                        needToCreate=false;
                        }
                    }
            if(needToCreate){
                Debug.Log("making category"+added.categoryName);
                ShopCategory temp=MakeNewCategory(added.categoryName);
                main.subCategories.Add(temp);
                Merge(temp,added);
                }
            }
        foreach(ShopItem itemToAdd in toAdd.items){
            main.items.Add(itemToAdd);
            }
        }

    private ShopCategory MakeNewCategory(string name){
        Debug.Log("making "+name);
        ShopCategory newCategory=new ShopCategory();
        newCategory.categoryName=name;
        newCategory.subCategories=new List<ShopCategory>();
        newCategory.items=new List<ShopItem>();
        return newCategory;
    }
}
