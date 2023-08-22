using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "New Furniture Database", menuName = "Data/Furniture Database")]
public class FurnitureDatabase : ScriptableObject
{
    [SerializeField]
    private List<FurnitureCategory> categoriesToLoad;
    [SerializeField]
    private List<Furniture> furnitureToLoad;

    private IDictionary<string,List<Furniture>> categories;
    private List<Furniture> furniture;
    
    [SerializeField]
    private bool shouldLoadAdressable =false;
    [SerializeField]
    private string AdressableCategoryLabel;
    [SerializeField]
    private string AdressableFurnitureLabel;

    public async Task  Setup(){
        categories=new Dictionary<string,List<Furniture>>();
        furniture=new List<Furniture>();
        foreach(FurnitureCategory cat in categoriesToLoad)
        {   
            AddCategory(cat);
        }

        foreach(Furniture fur in furnitureToLoad){
            AddFurniture(fur);
        }

        if(shouldLoadAdressable){
            await LoadAdressables();
        }
    }
    public async Task LoadAdressables()
    {           
                await LoadCategories();
                await LoadFurniture();
    }
    private async Task LoadCategories(){
            var gottenCategories = await Addressables.LoadResourceLocationsAsync(AdressableCategoryLabel, typeof(FurnitureCategory)).Task;
            List<Task<FurnitureCategory>> tasks = new List<Task<FurnitureCategory>>();
            foreach (var gottenCategory in gottenCategories)
            {
            tasks.Add(Addressables.LoadAssetAsync<FurnitureCategory>(gottenCategory).Task);
            }

            var loadedCategories = await Task.WhenAll(tasks);

            foreach (FurnitureCategory category in loadedCategories)
            {  
            AddCategory(category);
            }

    }
    private async Task LoadFurniture(){
        var gottenFurniture = await Addressables.LoadResourceLocationsAsync(AdressableFurnitureLabel, typeof(Furniture)).Task;
        List<Task<Furniture>> tasks = new List<Task<Furniture>>();
        foreach (var fur in gottenFurniture)
        {
        tasks.Add(Addressables.LoadAssetAsync<Furniture>(fur).Task);
        }

        var loadedFurniture = await Task.WhenAll(tasks);

        foreach (Furniture fur in loadedFurniture)
        { 
        AddFurniture(fur);
        }
    }

    private void AddCategory(FurnitureCategory input){
        foreach(string catName in categories.Keys)
            {
                if(catName !=input.catName)
                {
                continue;
                }
            else
            {
                foreach(Furniture furn in input.furniture)
                {
                    //list.furniture.Add(furn);
                    categories[input.catName].Add(furn);
                }
                return;
            }
        }
        List<Furniture> temp=new List<Furniture>();
        foreach(Furniture furn in input.furniture)
                {
                    //list.furniture.Add(furn);
                    temp.Add(furn);
                }
        categories.Add(input.catName,temp);

    }
    private void AddFurniture(Furniture input){
            furniture.Add(input);
    }
    public ICollection<string> GetCategoryNames(){
            return categories.Keys;
    }
    public ICollection<Furniture> GetCategory(string key){
        return categories[key];
    }
    public Furniture GetFurniture(string catName)
    {
        foreach (var furn in furniture)
        {
            
            if (furn.name == name)
            {
                return furn;
            }
        }
        return null;
        
    }
    
    public Furniture  GetFurniture(string category, int id){
        return categories[category][id];
    }
    public  Furniture GetFurniture(short id)
    {
        //Debug.Log("Getting from db" + id);
        return furniture[id];
    }
    public int GetKeyCount(){
        return categories.Count;
    }
    public int GetFurnitureCount(){
        return furniture.Count;
    }
    public  List<Furniture> GetList()
    {
        return furniture;
    }

    
}
